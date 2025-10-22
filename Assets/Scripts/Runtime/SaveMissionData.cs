using UnityEngine;

public class SaveMissionData
{
    static readonly string path = Application.persistentDataPath + "/Saves";

    public static void Save(string save)
    {
        // Save to Json 
    }

    public static string Load<T>(T t) where T : QuestInstance
    {
        // Load to Json 
        return "";
    }

    public static void Remove(object from)
    {
        // Delete json by id
    }

    public static void RemoveAll()
    {
        // Delete json all
    }
}