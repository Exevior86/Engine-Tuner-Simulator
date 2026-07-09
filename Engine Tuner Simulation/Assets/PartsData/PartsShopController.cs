using UnityEngine;
using System.Collections.Generic;

public class PartsShopController : MonoBehaviour
{
    [Header("Infrastructure Config")]
    [Tooltip("The parent object in your scroll view holding the Grid Layout Group component.")]
    public Transform gridContainer;

    [Tooltip("The UI Card Prefab asset template containing the PartUICard script component.")]
    public GameObject partCardPrefab;

    [Header("Catalog Tracking")]
    [Tooltip("Drag your generated ScriptableObject part files directly into this array list.")]
    public List<EnginePartData> activeCatalogItems = new List<EnginePartData>();

    private void Start()
    {
        PopulateShopInventory();
    }

    /// <summary>
    /// Cleans the display viewport and spawns interactive layout cards for every cataloged part asset.
    /// </summary>
    public void PopulateShopInventory()
    {
        // 1. Wipe out any existing placeholder design items inside the grid container
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Instantiate a fresh, responsive card asset for every component row in the listing
        foreach (EnginePartData part in activeCatalogItems)
        {
            GameObject newCard = Instantiate(partCardPrefab, gridContainer);
            PartUICard cardScript = newCard.GetComponent<PartUICard>();

            if (cardScript != null)
            {
                cardScript.SetupCard(part);
            }
        }
    }
}