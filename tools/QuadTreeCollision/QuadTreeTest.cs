
using UnityEngine;
using UnityEngine.UI;

public class QuadTreeTest : MonoBehaviour
{
    public GameObject RectTemplate;
    public GameObject CircleTemplate;

    public GameObject Offset;
    public Sprite sprite;
    private Canvas canvas;
    MonoObjectPool<RectCollisioner> rectPool;
    MonoObjectPool<CircleCollisioner> circlePool;

    QuadTreeCollisionSystem quadTreeCollisionSystem;
    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        quadTreeCollisionSystem = QuadTreeCollisionSystem.GetInstance(new TRect(canvas.pixelRect));
        rectPool = new MonoObjectPool<RectCollisioner>(RectTemplate, Offset.transform);
        circlePool = new MonoObjectPool<CircleCollisioner>(CircleTemplate, Offset.transform);

        for (int i = 0; i < 50; i++)
        {
            var obj = rectPool.Get();
            obj.gameObject.SetActive(true);
            obj.GetComponent<Image>().sprite = null;
            var x = Random.Range(canvas.pixelRect.xMin, canvas.pixelRect.xMax);
            var y = Random.Range(canvas.pixelRect.yMin, canvas.pixelRect.yMax);
            obj.transform.localPosition = new Vector3(x, y);
            obj.name = "Rect" + i;
            quadTreeCollisionSystem.Add(obj);
        }

        for (int i = 0; i < 50; i++)
        {
            var obj = circlePool.Get();
            obj.gameObject.SetActive(true);
            obj.GetComponent<Image>().sprite = sprite;
            var x = Random.Range(canvas.pixelRect.xMin, canvas.pixelRect.xMax);
            var y = Random.Range(canvas.pixelRect.yMin, canvas.pixelRect.yMax);
            obj.transform.localPosition = new Vector3(x, y);
            quadTreeCollisionSystem.Add(obj);
            obj.name = "Circle" + i;
        }
    }

    // Update is called once per frame
    void Update()
    {
        quadTreeCollisionSystem.Update();
    }
}
