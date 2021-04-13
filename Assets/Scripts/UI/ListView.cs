using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListView : MonoBehaviour
{
    public GameObject ListRow;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void AppendLine(string txt,Color txtcolr)
    {
        Instantiate(ListRow, transform);

    }
}
