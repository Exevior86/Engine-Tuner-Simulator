using System;
using System.Collections.Generic;

[Serializable]
public class UserProfile
{
    // General Logistics & Identity
    public string playerName = "Driver";
    public int careerLevel = 1;
    public int currentXP = 0;

    // Financial Records
    public float cashBalance = 5000f;

    // Inventory Tracking (Stores the string names of the .asset files)
    public List<string> purchasedPartIDs = new List<string>();
    public List<string> equippedPartIDs = new List<string>();

    /// <summary>
    /// Helper rule to check if a component is already owned.
    /// </summary>
    public bool HasPurchasedPart(string partID)
    {
        return purchasedPartIDs.Contains(partID);
    }
}