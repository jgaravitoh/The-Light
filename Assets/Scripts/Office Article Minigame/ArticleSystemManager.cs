using System;
using UnityEngine;

public class ArticleSystemManager : MonoBehaviour
{
    // --- Singleton ---
    public static ArticleSystemManager Instance { get; private set; }

    // Reset static state when entering Play Mode with "Enter Play Mode Options" (no domain reload).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
        TableLoaded = null;
    }

    // --- Config / Data ---
    private string address = "Assets/CSV Files/ArticlesMinigame.csv"; // Addressables TextAsset key
    public ArticleTable Table { get; private set; }

    // Fired when the CSV is loaded and parsed successfully.
    public static event Action<ArticleTable> TableLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Load(); // auto-load on startup
    }

    // Public: force reload (e.g., after changing Addressables key at runtime)
    public void Load() => LoadFromAddress(address);

    public void SetAddressAndReload(string newAddress)
    {
        address = newAddress;
        Load();
    }

    private void LoadFromAddress(string addr)
    {
        ArticleCSVLoader.LoadAsync(addr, table =>
        {
            Table = table;
            if (Table == null)
            {
                Debug.LogError("[ArticleSystemManager] Failed to load ArticleTable.");
                return;
            }
            TableLoaded?.Invoke(Table);
        });
    }

    // Convenience: compose the full article body from selected options.
    public bool TryCompose(int id, int opt1, int opt2, int opt3,
                           out string title, out string lede, out string body)
    {
        title = lede = body = null;
        if (Table == null) return false;
        if (!Table.TryGetIndex(id, out var idx)) return false;

        title = Table.Titles[idx];
        lede = Table.Ledes[idx];
        body = Table.ComposeBody(id, opt1, opt2, opt3);
        return true;
    }

    // Helpers to fetch slot options / viral index if you want to drive UI easily
    public string[] GetOptions(int id, int slot) =>
        Table == null ? Array.Empty<string>() : Table.GetSlotOptions(id, slot);

    public int GetViralIndex(int id, int slot) =>
        Table == null ? -1 : Table.GetViralIndex(id, slot);
}
