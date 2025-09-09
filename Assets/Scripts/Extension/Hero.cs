using UnityEngine;
public class Hero : MonoBehaviour
{
    private void Start()
    {
        this.gameObject.GetOrAdd<Hero>().OrNull();
    }
}
public static class GameObjectExtension
{
    public static T GetOrAdd<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if(!component) component = gameObject.AddComponent<T>();
        return component;
    }
    public static T OrNull <T> (this T obj) where T : Object => obj ? obj : null;
}
