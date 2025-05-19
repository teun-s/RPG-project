using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator animator;
    public Animator handsAnimator;
    public Animator axeAnimator;
    public SpriteRenderer handsRenderer;
    public SpriteRenderer axeRenderer;
    public float speed = 5f;
    private float lastMoveX = 1f;
    private float lastMoveY = -1f;
    private TreeResource currentTree;

    [SerializeField] private BankUI bankUI; // Serialized reference to BankUI
    private bool isNearBank = false; // Track if player is near the bank

    public enum AxeType { Iron, Steel, Bronze }
    private AxeType currentAxeType = AxeType.Iron;
    private bool isHoldingItem = false;
    private bool isChopping = false;
    private GatherableResource currentGatherable; // Track the nearest gatherable resource

    void Start()
    {
        handsRenderer = transform.Find("Hands").GetComponent<SpriteRenderer>();
        handsAnimator = transform.Find("Hands").GetComponent<Animator>();
        axeRenderer = transform.Find("Hands/Axe").GetComponent<SpriteRenderer>();
        axeAnimator = transform.Find("Hands/Axe").GetComponent<Animator>();
        UpdateAxeColor();

        // Check if bankUI is assigned
        if (bankUI == null)
        {
            Debug.LogError("BankUI not assigned in PlayerAnimationController!");
        }
    }

    void Update()
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            moveY = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            moveY = -1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            moveX = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            moveX = 1f;

        if (moveX != 0)
        {
            lastMoveX = moveX;
            lastMoveY = 0f;
        }
        if (moveY != 0)
        {
            lastMoveY = moveY;
            if (moveX == 0)
            {
                lastMoveX = 0f;
            }
        }

        Vector2 movement = new Vector2(moveX, moveY).normalized * speed * Time.deltaTime;
        transform.Translate(movement);

        bool isMoving = moveX != 0 || moveY != 0;
        float currentMoveX = isMoving ? moveX : (moveX != 0 ? moveX : lastMoveX);
        float currentMoveY = isMoving ? moveY : (moveY != 0 ? moveY : lastMoveY);

        if (currentMoveY != 0)
        {
            currentMoveX = 0f;
        }

        if (GetComponent<SpriteRenderer>() != null)
        {
            GetComponent<SpriteRenderer>().flipX = (lastMoveX < 0);
        }

        if (handsRenderer != null)
        {
            handsRenderer.flipX = (currentMoveX < 0 && currentMoveY == 0);
        }

        if (axeRenderer != null)
        {
            axeRenderer.flipX = (currentMoveX < 0 && currentMoveY == 0);
        }

        animator.SetBool("IsMoving", isMoving);
        handsAnimator.SetBool("IsMoving", isMoving);
        axeAnimator.SetBool("IsMoving", isMoving);

        animator.SetFloat("moveX", currentMoveX);
        animator.SetFloat("moveY", currentMoveY);
        handsAnimator.SetFloat("moveX", currentMoveX);
        handsAnimator.SetFloat("moveY", currentMoveY);
        axeAnimator.SetFloat("moveX", currentMoveX);
        axeAnimator.SetFloat("moveY", currentMoveY);

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {

            // Gather nearby resource if not holding an item
            if (currentGatherable != null && currentGatherable.isPlayerNearby)
            {
                Debug.Log("E key pressed. Gathering resource: " + currentGatherable.gameObject.name);
                currentGatherable.Gather();
                currentGatherable = null;
            }
            else
            {
                isHoldingItem = !isHoldingItem;
            }
        }
        handsAnimator.SetBool("IsHoldingItem", isHoldingItem);
        axeAnimator.SetBool("IsHoldingItem", isHoldingItem);

        bool isUsingAxe = Keyboard.current.nKey.isPressed;
        animator.SetBool("IsUsingAxe", isUsingAxe);
        handsAnimator.SetBool("IsUsingAxe", isUsingAxe);
        axeAnimator.SetBool("IsUsingAxe", isUsingAxe);

        if (axeRenderer != null)
        {
            axeRenderer.enabled = isHoldingItem;
        }

        if (isUsingAxe && currentTree != null && isHoldingItem)
        {
            currentTree.StartChopping();
        }
        else if (currentTree != null)
        {
            currentTree.StopChopping();
        }

        // Open the bank UI with 'B' key from anywhere
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            Debug.Log("B key pressed. Opening BankUI.");
            if(isNearBank && bankUI != null)
            {
                Debug.Log("Near bank, toggling bank UI");
                if (bankUI.gameObject.activeSelf)
                {
                    bankUI.CloseUI();
                    UIEventManager.Instance.NotifyBankClosed();
                }
                else
                {
                    bankUI.OpenUI();
                    UIEventManager.Instance.NotifyBankOpened();
                }
            }else if (!isNearBank)
            {
                Debug.Log("Player tried to open bank, but is not near");
            }else if (bankUI == null)
            {
                Debug.Log("bankUI is null, can't open bank");
            }
        }
        else if (Keyboard.current.bKey.wasPressedThisFrame && bankUI == null)
        {
            Debug.LogError("B key pressed, but bankUI is null!");
        }

        // Toggle chopping with 'N' key
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            isChopping = !isChopping;
            if (currentTree != null)
            {
                if (isChopping)
                {
                    currentTree.StartChopping();
                }
                else
                {
                    currentTree.StopChopping();
                }
            }
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            currentAxeType = AxeType.Iron;
            UpdateAxeColor();
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            currentAxeType = AxeType.Steel;
            UpdateAxeColor();
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            currentAxeType = AxeType.Bronze;
            UpdateAxeColor();
        }
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tree"))
        {
            currentTree = other.GetComponent<TreeResource>();
            Debug.Log("Entered tree trigger area.");
        }
        if (other.CompareTag("Bank"))
        {
            isNearBank = true;
            Debug.Log("Entered bank trigger area");
        }
        else if (other.CompareTag("Gatherable"))
        {
            currentGatherable = other.GetComponent<GatherableResource>();
            Debug.Log("Entered gatherable trigger area: " + other.gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Tree"))
        {
            if (currentTree != null)
            {
                currentTree.StopChopping();
                currentTree = null;
                Debug.Log("Exited tree trigger area.");
            }
        }
        if (other.CompareTag("Bank"))
        {
            isNearBank = false;
            Debug.Log("Excited bank trigger area..");
        }
        else if (other.CompareTag("Gatherable"))
        {
            if (currentGatherable != null)
            {
                currentGatherable = null;
                Debug.Log("Exited gatherable trigger area.");
            }
        }
    }

    private void UpdateAxeColor()
    {
        if (axeRenderer == null) return;

        switch (currentAxeType)
        {
            case AxeType.Iron:
                axeRenderer.color = Color.white;
                break;
            case AxeType.Steel:
                axeRenderer.color = new Color(0.8f, 0.8f, 1.0f);
                break;
            case AxeType.Bronze:
                axeRenderer.color = new Color(1.0f, 0.65f, 0.4f);
                break;
        }
    }

    public float GetLastMoveX()
    {
        return lastMoveX;
    }

    public float GetLastMoveY()
    {
        return lastMoveY;
    }
}