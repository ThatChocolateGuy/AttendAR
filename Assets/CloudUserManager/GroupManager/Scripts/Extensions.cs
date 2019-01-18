using UnityEngine;
using System.Collections;

public static class Extensions {

    public static T FindComponentInChildWithTag<T>(this GameObject parent, string tag) where T : Component
    {
        GameObject gameObj = parent.FindChildWithTag(tag);
        if(gameObj != null)
        {
            return gameObj.GetComponent<T>();
        }
        else
        {
            return null;
        }
    }

    public static GameObject FindChildWithTag(this GameObject parent, string tag)
    {
        return parent.transform.FindChildWithTag(tag);
    }

    public static GameObject FindChildWithTag(this Transform t, string tag)
    {
        foreach (Transform tr in t)
        {
            if (tr.transform.childCount > 0)
            {
                GameObject ret = tr.FindChildWithTag(tag);
                if (ret != null)
                    return ret;
            }

            if (tr.tag == tag)
            {
                return tr.gameObject;
            }
        }

        return null;
    }
}
