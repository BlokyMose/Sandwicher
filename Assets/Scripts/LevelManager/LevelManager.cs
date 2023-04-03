using Encore.Utility;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
using UnityUtility;
using static Sandwicher.Recipe;
using static SandwicherInputActions;
using static Sandwicher.SandwicherUtility;
using static UnityUtility.ComponentUtility;
using Unity.VisualScripting;

namespace Sandwicher
{
    [RequireComponent(typeof(AudioSourceRandom))]
    public class LevelManager : MonoBehaviour
    {
        #region [Classes]

        class Sandwich
        {
            public class IngredientCache
            {
                public Ingredient Ingredient;
                public IngredientShape CurrentShape;
                public int PosY;

                public IngredientCache(Ingredient ing, IngredientShape currentShape, int posY)
                {
                    Ingredient = ing;
                    CurrentShape = currentShape;
                    PosY = posY;
                }
            }

            public Transform Transform;
            Transform ingredientsParent;
            Vector2 size;
            List<IngredientCache> ingredientsCache = new();
            public List<Ingredient> Ingredients
            {
                get
                {
                    List<Ingredient> _ingredients = new();
                    foreach (var ing in ingredientsCache)
                        _ingredients.Add(ing.Ingredient);
                    return _ingredients;
                }
            }
            public int IngredientCount => ingredientsCache.Count;

            public Sandwich(Transform transform, Transform ingredientsParent, Vector2 size)
            {
                this.Transform = transform;
                this.ingredientsParent = ingredientsParent;
                this.size = size;
            }

            public void AddIngredient(Ingredient toAddIng, GameObject ingredientPrefab)
            {
                var ingSpriteGO = Instantiate(ingredientPrefab, ingredientsParent);
                ingSpriteGO.name = ingredientsCache.Count.ToString();
                ingSpriteGO.transform.localScale = Vector2.one;

                var rectTransform = ingSpriteGO.GetComponent<RectTransform>();
                rectTransform.sizeDelta = size;

                var imageComp = ingSpriteGO.GetComponent<Image>();
                imageComp.preserveAspect = true;
                if (ingredientsCache.Count == 0) // Exempt first ingredient to have height
                {
                    imageComp.sprite = toAddIng.Sprite;
                    ingSpriteGO.transform.localPosition = Vector2.zero;
                    ingredientsCache.Add(new IngredientCache(toAddIng, toAddIng.Shape, toAddIng.NextSpace.Height));
                }
                else
                {
                    var currentShape = toAddIng.Shape;
                    var previousIng = ingredientsCache.GetLast();

                    // Round the ingredient if it can be rounded and the last ingredient is round 
                    if (toAddIng.CanBeRounded && previousIng.CurrentShape == IngredientShape.Rounded)
                    {
                        imageComp.sprite = toAddIng.SpriteRounded;
                        currentShape = IngredientShape.Rounded;
                    }

                    // Flatten the ingredient if the shape is not round, it cannot be rounded,
                    // or the last ingredient is not round
                    else if (toAddIng.Shape != IngredientShape.Rounded)
                    {
                        imageComp.sprite = toAddIng.Sprite;
                        currentShape = IngredientShape.Flat;
                    }

                    // Sum all ingredients height
                    var posY = previousIng.PosY + toAddIng.Height.Height;

                    // Add extra rounded height if this ingredient is flat and the last one is round
                    if (previousIng.CurrentShape == IngredientShape.Rounded &&
                        previousIng.Ingredient.CanBeRounded &&
                        currentShape == IngredientShape.Flat)
                        posY += previousIng.Ingredient.ExtraRoundedHeight.Height;

                    ingSpriteGO.transform.localPosition = new Vector2(0, posY);
                    posY += toAddIng.NextSpace.Height;
                    ingredientsCache.Add(new IngredientCache(toAddIng, currentShape, posY));
                }
            }

            public void ClearIngredientsCache()
            {
                ingredientsCache.Clear();
            }

            public void DestroyIngredientsGO()
            {
                for (int i = ingredientsParent.childCount - 1; i >= 0; i--)
                    Destroy(ingredientsParent.GetChild(i).gameObject);
            }

            public int CountIngredientOf(Ingredient targetIng)
            {
                var count = 0;
                foreach (var ing in ingredientsCache)
                    if (ing.Ingredient == targetIng) count++;
                return count;
            }
        }

        class IngBut
        {
            GameObject go;
            CanvasGroup canvasGroup;
            public CanvasGroup CanvasGroup => canvasGroup;
            Animator animator;

            Ingredient ingredient;
            public Ingredient Ingredient => ingredient;
            Image countImage;
            GameObject countPanel;
            bool isCountPanelShown = false;
            public bool IsCountPanelShown => isCountPanelShown;
            int boo_show;

            public IngBut(GameObject go, CanvasGroup canvasGroup, Animator animator, Ingredient ingredient, Image countImage, GameObject countPanel)
            {
                boo_show = Animator.StringToHash(nameof(boo_show));
                this.go = go;
                this.canvasGroup = canvasGroup;
                this.animator = animator;
                this.ingredient = ingredient;
                this.countImage = countImage;
                this.countPanel = countPanel;
            }

            public void ShowCountPanel(bool isShown)
            {
                countPanel.SetActive(isShown);
                isCountPanelShown = isShown;
            }

            public void SetCount(Sprite countSprite)
            {
                countImage.sprite = countSprite;
            }

            public void Show(bool isShown)
            {
                animator.SetBool(boo_show, isShown);
                canvasGroup.interactable = isShown;
                canvasGroup.blocksRaycasts = isShown;
            }

        }

        class CustomerCache
        {
            public CustomerProfile Profile;
            public Character Character;
            public Order Order;
            public int PosIndex;
            Action<CustomerCache> OnOrder;
            List<QueuePosCache> QueuePosCache = new();
            List<Transform> ExitRout = new();
            Coroutine corMoving;

            public CustomerCache(CustomerProfile profile, Character character, Order order, List<QueuePosCache> queuePosCache, List<Transform> exitRoute)
            {
                Profile = profile;
                Order = order;
                Character = character;
                QueuePosCache = queuePosCache;
                ExitRout = exitRoute;
            }

            public void Init(MonoBehaviour context, Action<CustomerCache> onOrder)
            {
                OnOrder = onOrder;
                PosIndex = QueuePosCache.Count - 1;
                for (int i = 0; i < QueuePosCache.Count - 1; i++)
                    if (QueuePosCache[i + 1].IsOccupied)
                    {
                        PosIndex = i;
                        break;
                    }

                QueuePosCache[PosIndex].IsOccupied = true;
                corMoving = context.RestartCoroutine(MoveTo(QueuePosCache[PosIndex].Position, TryOrder), corMoving);
            }

            public void TryMoveToNextPos(MonoBehaviour context)
            {
                var nextPos = QueuePosCache.GetAt(PosIndex + 1, null);
                if (nextPos != null && !nextPos.IsOccupied)
                {
                    QueuePosCache[PosIndex].IsOccupied = false;
                    PosIndex++;
                    QueuePosCache[PosIndex].IsOccupied = true;
                    corMoving = context.RestartCoroutine(MoveTo(QueuePosCache[PosIndex].Position, TryOrder), corMoving);
                }
            }

            void TryOrder()
            {
                if (PosIndex == QueuePosCache.Count - 1)
                    OnOrder?.Invoke(this);
            }

            public IEnumerator MoveTo(Transform pos, Action onArrived)
            {
                while (Vector2.Distance(Character.transform.position, pos.position) > .025f)
                {
                    var marginY = pos.position.y - Character.transform.position.y;
                    var marginX = pos.position.x - Character.transform.position.x;
                    if (marginY > .025f)
                        this.Character.Move(MoveDir.Up);
                    else if (marginY < -.025f)
                        this.Character.Move(MoveDir.Down);
                    if (marginX > .025f)
                        this.Character.Move(MoveDir.Right);
                    else if (marginX < -.025f)
                        this.Character.Move(MoveDir.Left);

                    yield return null;
                }

                this.Character.Move(MoveDir.Stop);
                onArrived?.Invoke();
            }

            public IEnumerator ReturnHome()
            {
                QueuePosCache[PosIndex].IsOccupied = false;

                foreach (var pos in ExitRout)
                    yield return MoveTo(pos, null);

                Destroy(Character.gameObject);
            }
        }

        class QueuePosCache
        {
            public Transform Position;
            public bool IsOccupied;

            public QueuePosCache(Transform position)
            {
                Position = position;
                IsOccupied = false;
            }
        }

        private const float throwAwayAnimationDelay = 0.66f;

        #endregion

        #region [Vars: Properties]

        [Title("Game")]
        [SerializeField]
        bool isAutoSetup = true;

        [SerializeField]
        GameCache gameCache;

        [SerializeField]
        Level level;

        [SerializeField]
        float durationPerIngredient = 1.5f;

        [SerializeField]
        int maxPrice = 100;

        [SerializeField]
        Level nextLevel;


        [Title("Game UI")]
        [SerializeField]
        Animator gameCanvasAnimator;

        [SerializeField]
        Image doneButton;

        [SerializeField]
        Image cancelButton;

        [SerializeField]
        Transform ingButsTopRow;

        [SerializeField]
        Transform ingButsBottomRow;

        [SerializeField]
        Transform sandwichParent;

        [SerializeField]
        Vector2 sandwichSize = new(1028, 440);

        [Header("Cashier")]
        [SerializeField]
        Slider priceSlider;

        [SerializeField]
        RectTransform priceSliderHandler;

        [SerializeField]
        GOLister priceNumLister;

        [SerializeField]
        Image serveButton;

        [Header("Ing Composite")]
        [SerializeField]
        Transform ingCompPanel;

        [SerializeField]
        Transform ingCompTopRow;

        [SerializeField]
        Transform ingCompBottomRow;

        [Header("Profit & Rating")]
        [SerializeField]
        GOLister profitNumLister;

        [SerializeField]
        GOLister ratingNumLister;

        [SerializeField]
        GOLister profitNotifNumLister;

        [SerializeField]
        Animator profitNotifAnimator;

        [SerializeField]
        GOLister ratingNotifNumLister;

        [SerializeField]
        Animator ratingNotifAnimator;

        [SerializeField]
        Animator ratingFeedbackAnimator;

        [SerializeField]
        TextMeshProUGUI ratingFeedbackText;

        [Header("Customer Profile")]
        [SerializeField]
        Animator customerPanelAnimator;

        [SerializeField]
        TextMeshProUGUI customerNameText;

        [SerializeField]
        Image customerPicture;

        [SerializeField, LabelText("Tolerance Fill")]
        Image customerTimeToleranceFill;

        [SerializeField, LabelText("Tolerance Levels")]
        Transform customerTimeToleranceLevelsParent;

        [SerializeField, LabelText("Tolerance Levels BG")]
        Transform customerTimeToleranceLevelsParentBackground;

        [SerializeField, LabelText("Price Fill")]
        Image customerPriceToleranceFill;

        [SerializeField, LabelText("Price Levels")]
        Transform customerPriceToleranceLevelsParent;

        [SerializeField, LabelText("Price Levels BG")]
        Transform customerPriceToleranceLevelsParentBackground;

        [SerializeField]
        TextMeshProUGUI orderText;

        [Title("World: Customer")]
        [SerializeField]
        Transform customerSpawnPos;

        [SerializeField]
        List<Transform> queueRoute = new();

        [SerializeField]
        List<Transform> exitRoute = new();

        [Title("Menu")]
        [SerializeField]
        Image burgerButton;

        [SerializeField]
        GOLister soundButton;

        [SerializeField]
        Image homeButton;

        [SerializeField]
        Image quitButton;

        [Header("Recipe")]
        [SerializeField]
        Animator recipeCanvasAnimator;

        [SerializeField]
        TextMeshProUGUI recipeName;

        [SerializeField]
        TextMeshProUGUI recipePage;

        [SerializeField]
        Transform recipeSandwichParent;

        [SerializeField]
        Transform recipeIngredientsParent;

        [SerializeField]
        Image recipeMenuBG;

        [SerializeField]
        Image recipeNextPageButton;

        [SerializeField]
        Image recipeBackPageButton;

        [Title("Start Canvas")]
        [SerializeField]
        Animator startCanvasAnimator;

        [SerializeField]
        TextMeshProUGUI startBannerText;

        [SerializeField]
        Image startCanvasStartBut;

        [SerializeField]
        Transform startCanvasTodayMenuContent;

        [SerializeField]
        TextMeshProUGUI startCanvasCustomersText;

        [SerializeField]
        Transform startCanvasCustomersContent;

        [SerializeField]
        TextMeshProUGUI startCanvasIngredientsText;

        [SerializeField]
        Transform startCanvasIngredientsContent;

        [Title("Score Canvas")]
        [SerializeField]
        GOLister scoreCanvasLister;

        [SerializeField]
        Image scoreHomeButton;

        [SerializeField]
        Image scoreNextLevelButton;

        [Title("Resources")]
        [SerializeField]
        Character customerPrefab;

        [SerializeField]
        GameObject ingredientPrefab;

        [SerializeField]
        GameObject ingButPrefab;

        [SerializeField]
        GameObject sandwichPrefab;

        [SerializeField]
        GameObject recipeMenuSandwichPrefab;

        [SerializeField]
        GameObject recipeIngPanelPrefab;

        [SerializeField]
        GameObject todayMenuItemPrefab;

        [SerializeField]
        GameObject startCanvasCustomerIconPrefab;

        [SerializeField]
        GameObject startCanvasIngredientIconPrefab;

        [SerializeField]
        SandwicherSymbols symbols;

        [SerializeField]
        SandwicherColors colors;

        #endregion

        #region [Vars: Data Handlers]

        GameState state = GameState.Sandwich;
        public GameState State
        {
            get => state;
            private set
            {
                state = value;
                gameCanvasAnimator.SetInteger(int_state, (int)state);
            }
        }
        AudioSourceRandom audio;
        Sandwich sandwich;
        Sandwich recipeSandwich;
        float timer;
        int sandwichPrice;
        List<int> ratingsReceived = new();
        int profit;
        int orderServedCount;
        int orderCancelledCount;
        List<IngBut> ingButs = new();
        List<CustomerCache> customers = new();
        List<QueuePosCache> queuePosCache = new();
        int spawnedCustomerCount;

        Coroutine corTimer;
        Coroutine corThrowingAway;
        Coroutine corServing;
        int int_state, boo_show, tri_show; // Animator parameter

        readonly int maxIngredientCount = 9;
        float priceSliderMaxPosX;
        const float customerExitingDelay = 2f;
        bool isShowingQuitButton;
        bool isShowingRecipeMenu;
        int recipePageIndex;

        List<Recipe> possibleRecipes = new();

        #endregion

        #region [Methods: Setup]

        void Awake()
        {
            LevelTransition.HideAllTransition();
            audio = GetComponent<AudioSourceRandom>();

            int_state = Animator.StringToHash(nameof(int_state));
            boo_show = Animator.StringToHash(nameof(boo_show));
            tri_show = Animator.StringToHash(nameof(tri_show));

            if (gameCache != null)
            {
                if (gameCache.GameLevel != null && level == null)
                    level = gameCache.GameLevel;
                if (nextLevel == null)
                {
                    var levelIndex = gameCache.Levels.IndexOf(level);
                    if (levelIndex != -1 && levelIndex < gameCache.Levels.Count - 1)
                        nextLevel = gameCache.Levels[levelIndex + 1];
                }
            }
            possibleRecipes = level.GetUniqueRecipes();
            ingCompPanel.gameObject.SetActive(false);
            foreach (var pos in queueRoute)
                queuePosCache.Add(new QueuePosCache(pos));

            sandwich = MakeSandwich(sandwichPrefab, sandwichParent);
            priceSliderMaxPosX = priceSliderHandler.parent.GetComponent<RectTransform>().rect.width;
            priceSlider.onValueChanged.AddListener(OnPriceSlider);

            if (isAutoSetup) SetupMenus();
            DebugLog.Notice("Load", level.LevelDisplayName);

        }

        void OnDestroy()
        {
            priceSlider.onValueChanged.RemoveListener(OnPriceSlider);
        }

        void OnPriceSlider(float value)
        {
            priceSliderHandler.anchoredPosition = new Vector2(priceSliderMaxPosX * value, priceSliderHandler.anchoredPosition.y);
            sandwichPrice = Mathf.CeilToInt(maxPrice * value);
            DisplayNum(priceNumLister, sandwichPrice, colors.Black, symbols);
        }

        public void SetupMenus(bool setupSideMenu = true, bool setupRecipeCanvas = true, bool setupStartCanvas = true)
        {
            if (setupSideMenu)
                SetupSideMenu(burgerButton, soundButton.GetComponent<Image>(), homeButton, quitButton);

            if (setupRecipeCanvas)
                SetupRecipeCanvas(recipeCanvasAnimator, recipeMenuBG, recipeNextPageButton, recipeBackPageButton, recipePage, possibleRecipes);

            if (setupStartCanvas)
                SetupStartCanvas(
                    startCanvasAnimator,
                    startBannerText,
                    startCanvasStartBut,
                    level,

                    todayMenuItemPrefab,
                    recipeMenuSandwichPrefab,
                    startCanvasTodayMenuContent,

                    startCanvasCustomerIconPrefab,
                    startCanvasCustomersText,
                    startCanvasCustomersContent,

                    startCanvasIngredientIconPrefab,
                    startCanvasIngredientsText,
                    startCanvasIngredientsContent);

            SetupScoreCanvas(scoreCanvasLister, scoreHomeButton, scoreNextLevelButton);
        }

        void SetupGameCanvas(Animator gameCanvasAnimator, Image cancelBut, Image doneBut, Image serveBut)
        {
            gameCanvasAnimator.gameObject.SetActive(true);

            cancelBut.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: ThrowAwaySandwich);
            doneBut.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: ShowCashier);
            serveBut.AddEventTriggers(onDown: PlayClickIn, onUp: PlayKaching, onClick: Serve);
        }

        void SetupSideMenu(Image burgerButton, Image soundButton, Image homeButton, Image quitButton)
        {
            burgerButton.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: ToggleRecipeMenu);
            soundButton.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: SwitchSoundVolume);
            homeButton.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: ToggleQuitButton);
            quitButton.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: LoadMainMenu);
        }

        void SetupIngredientButtons(List<IngBut> ingButs, Transform ingButsTopRow, Transform ingButsBottomRow, List<Ingredient> ingredients)
        {
            ingButsTopRow.DestroyChildren();
            ingButsBottomRow.DestroyChildren();

            var ingTopRowMaxCount = Mathf.FloorToInt(ingredients.Count / 2f);
            var ingBottomRowMaxCount = Mathf.CeilToInt(ingredients.Count / 2f);
            int index = 0;
            foreach (var ing in ingredients)
            {
                if (index < ingTopRowMaxCount)
                    ingButs.Add(MakeIngButton(ing, ingButsTopRow, AddIngredientToSandwich));
                else if (index < ingTopRowMaxCount + ingBottomRowMaxCount)
                    ingButs.Add(MakeIngButton(ing, ingButsBottomRow, AddIngredientToSandwich));
                ingButs.GetLast().Show(false);
                index++;
            }

            IngBut MakeIngButton(Ingredient ing, Transform parent, Action<Ingredient> onAddIngredient)
            {
                var button = Instantiate(ingButPrefab, parent);
                if (button.TryGetComponent<GOLister>(out var lister))
                {
                    if (lister.TryGet("icon", out var icon))
                    {
                        var iconImage = icon.GetComponent<Image>();
                        iconImage.sprite = ing.Icon;
                    }

                    DisplayNum(lister, ing.Price, colors.Black, symbols);

                    var butImage = button.GetComponent<Image>();
                    butImage.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: () => { onAddIngredient(ing); });


                    if (lister.TryGet("countPanel", out var countPanel) &&
                        lister.TryGet("count", out var count))
                    {
                        countPanel.SetActive(false);
                        var canvasGroup = button.GetComponent<CanvasGroup>();
                        var animator = button.GetComponent<Animator>();
                        var ingBut = new IngBut(button, canvasGroup, animator, ing, count.GetComponent<Image>(), countPanel);
                        return ingBut;
                    }
                }

                return null;
            }
        }

        void SetupRecipeCanvas(Animator recipeCanvas, Image recipeMenuBG, Image nextButton, Image backButton, TextMeshProUGUI recipePage, List<Recipe> recipes)
        {
            recipeMenuBG.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: HideRecipeMenu);
            nextButton.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: NextRecipePage);
            backButton.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: BackRecipePage);

            recipeSandwich = MakeSandwich(recipeMenuSandwichPrefab, recipeSandwichParent);
            recipeSandwich.Transform.name = "RecipeMenu_Sandwich";
            recipePageIndex = 0;
            ShowRecipePage(recipePageIndex);

            recipeCanvas.gameObject.SetActive(false);
        }

        void SetupStartCanvas(
            Animator startCanvasAnimator,
            TextMeshProUGUI startBannerText,
            Image startBut,
            Level level,

            GameObject todayMenuItemPrefab,
            GameObject sandwichPrefab,
            Transform todayMenuContent,

            GameObject customerIconPrefab,
            TextMeshProUGUI customersText,
            Transform customersContent,

            GameObject ingredientIconPrefab,
            TextMeshProUGUI ingredientsText,
            Transform ingredientsContent)
        {
            startCanvasAnimator.gameObject.SetActive(true);
            startBannerText.text = $"[{level.LevelDisplayName}]\n<size=54>STARTED!";
            startBut.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: StartGame);

            todayMenuContent.DestroyChildren();
            foreach (var recipe in level.GetUniqueRecipes())
            {
                var lister = Instantiate(todayMenuItemPrefab, todayMenuContent).GetComponent<GOLister>();
                if (lister.TryGet("name", out var name) &&
                    lister.TryGet("desc", out var desc) &&
                    lister.TryGet("sandwich", out var sandwichParent))
                {
                    name.GetComponent<TextMeshProUGUI>().text = recipe.RecipeName;
                    desc.GetComponent<TextMeshProUGUI>().text = recipe.Desc;

                    sandwichParent.transform.DestroyChildren();
                    var sandwich = MakeSandwich(sandwichPrefab, sandwichParent.transform);
                    for (int i = recipe.IngredientPositions.Count - 1; i >= 0; i--)
                        sandwich.AddIngredient(recipe.IngredientPositions[i].Ingredient, ingredientPrefab);
                }
            }

            customersContent.DestroyChildren();
            foreach (var customer in level.CustomerProfiles)
            {
                var lister = Instantiate(customerIconPrefab, customersContent).GetComponent<GOLister>();
                if (lister.TryGet("icon", out var icon))
                {
                    icon.GetComponent<Image>().sprite = customer.Icon;
                }
            }

            ingredientsContent.DestroyChildren();
            foreach (var ing in level.Ingredients)
            {
                var lister = Instantiate(ingredientIconPrefab, ingredientsContent).GetComponent<GOLister>();
                if (lister.TryGet("icon", out var icon))
                {
                    icon.GetComponent<Image>().sprite = ing.Icon;
                }
            }

        }

        public void StartGame()
        {
            startCanvasAnimator.SetBool(boo_show, true);
            startCanvasStartBut.enabled = false;

            SetupCustomers(customerPanelAnimator, customers, level.CustomerProfiles, level.Orders, customerSpawnPos, queuePosCache);
            SetupGameCanvas(gameCanvasAnimator, cancelButton, doneButton, serveButton);
            SetupIngredientButtons(ingButs, ingButsTopRow, ingButsBottomRow, level.Ingredients);
        }

        void SetupScoreCanvas(GOLister scoreCanvas, Image homeButton, Image nextLevelButton)
        {
            scoreCanvas.gameObject.SetActive(false);
            homeButton.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: LoadMainMenu);
            if (nextLevel != null)
                nextLevelButton.AddEventTriggers(onDown: PlayClickIn, onUp: PlayClickOut, onClick: LoadNextLevel);
            else
                nextLevelButton.gameObject.SetActive(false);

            if (scoreCanvas.TryGet("clearText", out var clearText))
            {
                clearText.GetComponent<TextMeshProUGUI>().text = $"[{level.LevelDisplayName}]\r\n<size=54>CLEARED!";
            }
        }

        Sandwich MakeSandwich(GameObject prefab, Transform parent)
        {
            parent.DestroyChildren();
            var sandwichGO = Instantiate(prefab, parent);
            var lister = sandwichGO.GetComponent<GOLister>();
            var ingredientsParent = lister.Get("ingredients").transform;
            ingredientsParent.DestroyChildren();
            return new Sandwich(sandwichGO.transform, ingredientsParent, sandwichSize);
        }

        void SetupCustomers(Animator customerAnimator, List<CustomerCache> customers, List<CustomerProfile> profiles, List<Order> orders, Transform spawnPos, List<QueuePosCache> queuePosCache)
        {
            customerAnimator.gameObject.SetActive(true);
            StartCoroutine(Update());
            IEnumerator Update()
            {
                var period = 2.5f;
                var profileIndex = 0;
                var orderIndex = 0;
                var time = period;
                var i = 0;
                while (i < queuePosCache.Count && spawnedCustomerCount < level.CustomerCount)
                {
                    if (time >= period)
                    {
                        SpawnCustomer(customers, customerPrefab, profiles[profileIndex], orders[orderIndex], spawnPos, queuePosCache);
                        profileIndex = (profileIndex + 1) % profiles.Count;
                        orderIndex = (orderIndex + 1) % orders.Count;
                        time = 0;
                        i++;
                    }
                    time += Time.deltaTime * Time.timeScale;
                    yield return null;
                }
            }
        }

        #endregion

        #region [Methods: Game]

        void SpawnCustomer(List<CustomerCache> customers, Character prefab, CustomerProfile profile, Order order, Transform spawnPos, List<QueuePosCache> queuePosCache)
        {
            if (queuePosCache[0].IsOccupied) return;

            var character = Instantiate(prefab);
            character.gameObject.name = "Cus_" + spawnedCustomerCount;
            character.transform.position = spawnPos.position;

            var sLib = character.GetComponent<SpriteLibrary>();
            sLib.spriteLibraryAsset = profile.SLib;

            var customerCache = new CustomerCache(profile, character, order, queuePosCache, exitRoute);
            customerCache.Init(this, ReceiveOrder);
            customers.Add(customerCache);
            spawnedCustomerCount++;
        }

        void AddIngredientToSandwich(Ingredient ing)
        {
            if (sandwich == null) return;
            if (sandwich.IngredientCount >= maxIngredientCount) return;

            sandwich.AddIngredient(ing, ingredientPrefab);
            foreach (var ingBut in ingButs)
            {
                if (ingBut.Ingredient == ing)
                {
                    ingBut.ShowCountPanel(true);
                    ingBut.SetCount(symbols.Nums.GetAt(sandwich.CountIngredientOf(ing), 0));
                    break;
                }
            }

            if (sandwich.IngredientCount == maxIngredientCount)
                foreach (var ingBut in ingButs)
                    ingBut.Show(ingBut.IsCountPanelShown);
        }

        void ReceiveOrder(CustomerCache customer)
        {
            customerPanelAnimator.SetBool(boo_show, true);

            var customerProfile = customer.Profile;
            var order = customer.Order;

            customerNameText.text = customerProfile.DisplayName;
            customerPicture.sprite = customerProfile.Icon;
            orderText.text = order.GetStatementText();
            ActivateChildren(customerTimeToleranceLevelsParent, Mathf.FloorToInt(customerProfile.WaitTolerance.ToleratedRange * 10));
            ActivateChildren(customerTimeToleranceLevelsParentBackground, Mathf.FloorToInt(customerProfile.WaitTolerance.ToleratedRange * 10));
            ActivateChildren(customerPriceToleranceLevelsParent, Mathf.FloorToInt(customerProfile.PriceTolerance.ToleratedRange * 10));
            ActivateChildren(customerPriceToleranceLevelsParentBackground, Mathf.FloorToInt(customerProfile.PriceTolerance.ToleratedRange * 10));

            corTimer = this.RestartCoroutine(CountingTimer(), corTimer);
            IEnumerator CountingTimer()
            {
                yield return new WaitForSeconds(2.5f);
                State = GameState.Sandwich;

                foreach (var ingBut in ingButs)
                {
                    ingBut.Show(true);
                    ingBut.ShowCountPanel(false);
                    ingBut.SetCount(symbols.Nums[0]);
                }

                var maxTime = order.Recipe.IngredientPositions.Count * durationPerIngredient;
                maxTime += maxTime * customerProfile.WaitTolerance.RejectedRange;
                timer = maxTime;
                PlayBell();
                while (state != GameState.Serve)
                {
                    customerTimeToleranceFill.fillAmount = timer / maxTime;
                    timer -= Time.deltaTime * Time.timeScale;
                    if (timer < 0)
                    {
                        CancelOrder();
                        break;
                    }

                    yield return null;
                }
            }
        }

        void ShowCashier()
        {
            State = GameState.Cashier;
            foreach (var ingBut in ingButs)
                ingBut.Show(ingBut.IsCountPanelShown);
        }

        void Serve()
        {
            if (state == GameState.Serve) return;
            State = GameState.Serve;
            orderServedCount++;

            #region [Calculate Rating]

            var rating = 5;
            RatingFeedback ratingFeedback = RatingFeedback.None;
            var debug = "";
            if (HasSandwichIngredientsError(sandwich, customers[0].Order, out var errorCount))
            {
                debug += "Sandwich error : " + errorCount + "\n";
                rating -= errorCount;
                ratingFeedback += (int)RatingFeedback.WrongRecipe;
            }
            if (!IsServedOnTime(timer, durationPerIngredient, customers[0].Order, customers[0].Profile))
            {
                debug += "Not served on time\n";
                rating--;
                ratingFeedback += (int)RatingFeedback.Late;
            }
            if (!IsPriceReasonable(sandwichPrice, sandwich, customers[0].Profile))
            {
                debug += "Unreasonable price\n";
                rating--;
                ratingFeedback += (int)RatingFeedback.Pricey;
                customerPriceToleranceFill.fillAmount = .5f;
            }
            else
            {
                customerPriceToleranceFill.fillAmount = 1f;
            }
            rating = (rating < 0) ? 0 : rating;
            DebugLog.Color(DebugLog.ColorName.Cyan, "Rating", rating + "\n" + debug);

            #endregion


            ShowRatingFeedback(ratingFeedback);
            AddRating(rating);
            AddProfit(GetProfit(sandwichPrice, sandwich));

            corServing = this.RestartCoroutine(CustomerExiting(customerExitingDelay), corServing);

            #region [Methods: Rating Evaluation]


            bool HasSandwichIngredientsError(Sandwich sandwich, Order order, out int errorCount)
            {
                errorCount = 0;
                var checkIngredients = new List<Ingredient>(order.Recipe.Ingredients);
                for (int index = 0; index < sandwich.IngredientCount; index++)
                {
                    var ing = sandwich.Ingredients[index];
                    if (index == 0) // Check ingredient at the bottom match the order
                    {
                        if (order.TryGetIngredientByIndex(OrderIngIndex.Bottom, out var orderedBottomIng))
                            errorCount += (ing != orderedBottomIng) ? 1 : 0;
                    }
                    else if (index == sandwich.IngredientCount - 1) // Check ingredient at the top match the order
                    {
                        if (order.TryGetIngredientByIndex(OrderIngIndex.Top, out var orderedTopIng))
                            errorCount += (ing != orderedTopIng) ? 1 : 0;
                    }
                    else
                    {
                        if (!order.HasIngredient(ing)) // Unordered ingredient is inside the sandwich
                            errorCount++;
                        else if (!checkIngredients.Has(ing)) // Ingredient count is more than orderer
                            errorCount++;
                        else
                            checkIngredients.Remove(ing);
                    }
                }


                // Check ordered ingredient inside the sandwich
                foreach (var orderedIng in order.Recipe.IngredientPositions)
                    errorCount += (!sandwich.Ingredients.Has(orderedIng.Ingredient)) ? 1 : 0;

                return errorCount > 0;
            }

            bool IsServedOnTime(float timeLeft, float durationPerIngredient, Order order, CustomerProfile customerProfile)
            {
                var idealDuration = order.Recipe.IngredientPositions.Count * durationPerIngredient;
                var maxDuration = idealDuration * customerProfile.WaitTolerance.RejectedRange;
                var toleratedDuration = idealDuration * customerProfile.WaitTolerance.ToleratedRange;
                var durationMaking = maxDuration - timeLeft;
                return durationMaking <= toleratedDuration;
            }

            bool IsPriceReasonable(int price, Sandwich sandwich, CustomerProfile customerProfile)
            {
                var maxPrice = 0;
                foreach (var ing in sandwich.Ingredients)
                    maxPrice += ing.Price;

                maxPrice += Mathf.CeilToInt(maxPrice * customerProfile.PriceTolerance.ToleratedRange);
                return price <= maxPrice;
            }


            #endregion
        }

        void CancelOrder()
        {
            if (sandwich == null) return;
            if (state == GameState.Serve) return;
            State = GameState.Serve;
            orderCancelledCount++;

            corServing = this.RestartCoroutine(Delay(), corServing);

            IEnumerator Delay()
            {
                AddProfit(GetProfit(0, sandwich));
                AddRating(0);

                corThrowingAway = this.RestartCoroutine(ThrowingAwaySandwichForCancelling(throwAwayAnimationDelay), corThrowingAway);
                yield return corThrowingAway;


                yield return CustomerExiting(customerExitingDelay);
            }
        }

        void ThrowAwaySandwich()
        {
            if (sandwich == null) return;
            if (state == GameState.ThrowAway) return;

            AddProfit(GetProfit(0, sandwich));
            corThrowingAway = this.RestartCoroutine(ThrowingAwaySandwich(throwAwayAnimationDelay), corThrowingAway);
        }

        IEnumerator ThrowingAwaySandwich(float delay)
        {
            State = GameState.ThrowAway;
            cancelButton.enabled = false;
            foreach (var ingBut in ingButs)
                ingBut.Show(false);

            sandwich.ClearIngredientsCache();

            yield return new WaitForSeconds(delay);

            sandwich.DestroyIngredientsGO();

            State = GameState.Sandwich;
            cancelButton.enabled = true;
            foreach (var ingBut in ingButs)
            {
                ingBut.ShowCountPanel(false);
                ingBut.SetCount(symbols.Nums[0]);
                ingBut.Show(true);
            }
        }

        IEnumerator ThrowingAwaySandwichForCancelling(float delay)
        {
            State = GameState.ThrowAway;
            cancelButton.enabled = false;
            foreach (var ingBut in ingButs)
                ingBut.Show(false);

            sandwich.ClearIngredientsCache();

            yield return new WaitForSeconds(delay);

            sandwich.DestroyIngredientsGO();

            cancelButton.enabled = true;
            foreach (var ingBut in ingButs)
            {
                ingBut.ShowCountPanel(false);
                ingBut.SetCount(symbols.Nums[0]);
            }
        }

        IEnumerator CustomerExiting(float delay)
        {
            customerPanelAnimator.SetBool(boo_show, false);
            yield return new WaitForSeconds(delay / 2);
            StartCoroutine(customers[0].ReturnHome());
            yield return new WaitForSeconds(delay / 2);
            PrepareNextCustomer();
        }

        void AddProfit(int orderProfit)
        {
            if (orderProfit == 0) return;

            profit += orderProfit;
            DisplayNumSR(profitNotifNumLister, Mathf.Abs(orderProfit), colors.Black, symbols);
            if (profitNotifNumLister.TryGet("plusMinus", out var plusMinusNotif))
                plusMinusNotif.GetComponent<SpriteRenderer>().sprite = orderProfit >= 0 ? symbols.Plus : symbols.Minus;
            profitNotifAnimator.SetTrigger(tri_show);

            DisplayNumSR(profitNumLister, Mathf.Abs(profit), colors.Black, symbols);
            if (profitNumLister.TryGet("plusMinus", out var plusMinus))
                plusMinus.GetComponent<SpriteRenderer>().sprite = profit >= 0 ? symbols.Plus : symbols.Minus;

        }

        void AddRating(int rating)
        {
            DisplayNumSR(ratingNotifNumLister, rating * 10, colors.Black, symbols, null, symbols.Nums[0]);
            ratingNotifAnimator.SetTrigger(tri_show);
            ratingsReceived.Add(rating);

            float averageRating = GetAverageRating();

            DisplayNumSR(ratingNumLister, Mathf.CeilToInt(averageRating), colors.Black, symbols, null, symbols.Nums[0]);
            if (ratingNumLister.TryGet("ratingSymbol", out var ratingSymbol))
                ratingSymbol.GetComponent<SpriteRenderer>().sprite =
                    averageRating >= 40
                    ? symbols.RatingHappy : averageRating >= 25
                    ? symbols.RatingOk
                    : symbols.RatingSad;
        }

        void ShowRatingFeedback(RatingFeedback feedback)
        {
            ratingFeedbackText.text = "";
            ratingFeedbackText.text += feedback.HasFlag(RatingFeedback.WrongRecipe) ? "Wrong Recipe\n" : "";
            ratingFeedbackText.text += feedback.HasFlag(RatingFeedback.Late) ? "Late\n" : "";
            ratingFeedbackText.text += feedback.HasFlag(RatingFeedback.Pricey) ? "Pricey\n" : "";
            ratingFeedbackAnimator.SetTrigger(tri_show);
        }

        float GetAverageRating()
        {
            var averageRating = 0f;
            foreach (var r in ratingsReceived)
                averageRating += r;
            averageRating = averageRating / ratingsReceived.Count * 10;
            return averageRating;
        }

        void PrepareNextCustomer()
        {
            foreach (var ingBut in ingButs)
                ingBut.Show(false);

            sandwich.ClearIngredientsCache();
            sandwich.DestroyIngredientsGO();
            sandwichPrice = 0;
            priceSlider.value = 0f;
            customerTimeToleranceFill.fillAmount = 1f;
            customerPriceToleranceFill.fillAmount = 1f;
            DisplayNum(priceNumLister, 0, colors.Black, symbols);

            customers.RemoveAt(0);
            foreach (var customer in customers)
                customer.TryMoveToNextPos(this);
            if (spawnedCustomerCount < level.CustomerCount)
                SpawnCustomer(customers, customerPrefab, level.CustomerProfiles.GetRandom(), level.Orders.GetRandom(), customerSpawnPos, queuePosCache);
            else if (orderServedCount + orderCancelledCount >= level.CustomerCount)
                ShowScoreCanvas();
        }

        int GetProfit(int price, Sandwich sandwich)
        {
            var sandwichPrice = 0;
            foreach (var ing in sandwich.Ingredients)
                sandwichPrice += ing.Price;

            return price - sandwichPrice;
        }

        void ShowScoreCanvas()
        {
            scoreCanvasLister.gameObject.SetActive(true);
            if (scoreCanvasLister.TryGet("values", out var values))
            {
                var averageRating = GetAverageRating();
                var averageRatingString = averageRating.ToString();
                if (averageRatingString.Length == 1)
                    averageRatingString = "0." + averageRatingString;
                else
                    averageRatingString = averageRatingString[0] + "." + averageRatingString[1];

                var valuesData = DateTime.Now + "\n" +
                                orderServedCount + "\n" +
                                orderCancelledCount + "\n" +
                                (orderServedCount + orderCancelledCount) + "\n\n" +
                                averageRatingString + "\n" +
                                $"<size=48><b>{profit}</size></b>";

                values.GetComponent<TextMeshProUGUI>().text = valuesData;
            }
        }

        #endregion


        #region [Methods: Audio]

        public void PlayClickIn() => audio.PlayOneClipFromPack("clickIn");
        public void PlayClickOut() => audio.PlayOneClipFromPack("clickOut");
        public void PlayKaching() 
        {
            audio.PlayOneClipFromPack("clickOut");
            audio.PlayOneClipFromPack("kaching");
        }
        public void PlayBell() => audio.PlayOneClipFromPack("bell");


        #endregion


        #region [Methods: Menu]

        public void ToggleRecipeMenu()
        {
            if (!isShowingRecipeMenu) ShowRecipeMenu();
            else HideRecipeMenu();
        }

        public void ShowRecipeMenu()
        {
            Time.timeScale = 0;
            isShowingRecipeMenu = true;
            recipeCanvasAnimator.gameObject.SetActive(true);
            recipeMenuBG.gameObject.SetActive(true);
            recipeCanvasAnimator.SetBool(boo_show, true);
        }

        public void HideRecipeMenu()
        {
            Time.timeScale = 1;
            isShowingRecipeMenu = false;
            recipeMenuBG.gameObject.SetActive(false);
            recipeCanvasAnimator.SetBool(boo_show, false);
        }

        public void NextRecipePage()
        {
            recipePageIndex = (recipePageIndex + 1) % possibleRecipes.Count;
            ShowRecipePage(recipePageIndex);
        }

        public void BackRecipePage()
        {
            recipePageIndex = (recipePageIndex - 1 + possibleRecipes.Count) % possibleRecipes.Count;
            ShowRecipePage(recipePageIndex);
        }

        public void ShowRecipePage(int pageIndex)
        {
            if (!possibleRecipes.HasIndex(pageIndex)) return;

            var recipe = possibleRecipes[recipePageIndex];
            recipePage.text = "Page\n" + (pageIndex + 1) + "/" + possibleRecipes.Count;
            recipeName.text = recipe.RecipeName;

            recipeIngredientsParent.DestroyChildren();
            recipeSandwich.ClearIngredientsCache();
            recipeSandwich.DestroyIngredientsGO();

            int index = 0;
            foreach (var ing in recipe.Ingredients)
            {
                var ingGO = Instantiate(recipeIngPanelPrefab, recipeIngredientsParent);
                var lister = ingGO.GetComponent<GOLister>();
                if (lister.TryGet("text", out var text) &&
                    lister.TryGet("icon", out var icon))
                {
                    text.GetComponent<TextMeshProUGUI>().text = ing.IngName;
                    icon.GetComponent<Image>().sprite = ing.Icon;
                }

                index++;
            }

            for (int i = recipe.IngredientPositions.Count - 1; i >= 0; i--)
                recipeSandwich.AddIngredient(recipe.IngredientPositions[i].Ingredient, ingredientPrefab);
            
        }

        SoundVolume soundVolume;
        public void SwitchSoundVolume()
        {
            soundVolume = (SoundVolume)((int)(soundVolume + 1) % Enum.GetNames(typeof(SoundVolume)).Length);
            if (soundButton.TryGet("top", out var top) &&
                soundButton.TryGet("bottom", out var bottom) &&
                soundButton.TryGet("clickColor", out var clickColor))
            {
                switch (soundVolume)
                {
                    case SoundVolume.Full:
                        top.GetComponent<Image>().sprite = symbols.SoundFull;
                        clickColor.GetComponent<Image>().sprite = symbols.SoundFull;
                        bottom.GetComponent<Image>().sprite = symbols.SoundFullBottom;
                        break;
                    case SoundVolume.Medium:
                        top.GetComponent<Image>().sprite = symbols.SoundMedium;
                        clickColor.GetComponent<Image>().sprite = symbols.SoundMedium;
                        bottom.GetComponent<Image>().sprite = symbols.SoundMediumBottom;
                        break;
                    case SoundVolume.Quiet:
                        top.GetComponent<Image>().sprite = symbols.SoundQuiet;
                        clickColor.GetComponent<Image>().sprite = symbols.SoundQuiet;
                        bottom.GetComponent<Image>().sprite = symbols.SoundQuietBottom;
                        break;
                    case SoundVolume.Mute:
                        top.GetComponent<Image>().sprite = symbols.SoundMute;
                        clickColor.GetComponent<Image>().sprite = symbols.SoundMute;
                        bottom.GetComponent<Image>().sprite = symbols.SoundMuteBottom;
                        break;
                    default:
                        break;
                }
            }
        }

        public void LoadMainMenu() => LoadLevel(gameCache.MainMenu);

        public void LoadNextLevel()
        {
            gameCache.GameLevel = nextLevel;
            LoadLevel(nextLevel);
        }

        public void ToggleQuitButton()
        {
            isShowingQuitButton = !isShowingQuitButton;
            if (isShowingQuitButton) ShowQuitButton();
            else HideQuitButton();  
        }

        public void ShowQuitButton()
        {
            var animator = quitButton.GetComponent<Animator>();
            animator.SetBool(boo_show, true);
        }

        public void HideQuitButton()
        {
            var animator = quitButton.GetComponent<Animator>();
            animator.SetBool(boo_show, false);
        }

        #endregion

        #region [Methods: Utility]

        void ActivateChildren(Transform parent, int count)
        {
            for (int i = 0; i < parent.childCount; i++)
                parent.GetChild(i).gameObject.SetActive(i<count);
        }

        public void LoadLevel(Level level)
        {
            var transition = Instantiate(gameCache.LevelTransition);
            transition.OnLoad += () => SceneManager.LoadScene(level.SceneName);
        }

        #endregion


    }
}
