using System;
using UnityEngine;

public class UIEventManager : MonoBehaviour
{
    public static UIEventManager Instance { get; private set; }
    public event Action OnUIViewSwitched; // Fired when switching views (e.g., inventory to skills)
    public event Action OnUIClosed;       // Fired when the UI is closed
    public event Action OnBankOpened;
    public event Action OnBankClosed;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void NotifyBankOpened()
    {
        OnBankOpened.Invoke();
    }

    public void NotifyBankClosed()
    {
        OnBankClosed.Invoke();
    }
    public void NotifyViewSwitched()
    {
        OnUIViewSwitched?.Invoke();
    }

    public void NotifyUIClosed()
    {
        OnUIClosed?.Invoke();
    }
}
