using UnityEngine;

public class RandomPos : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    private float speed = 2f;
    private float waitTime = 1f;
    private bool isMoving = false;

    void Start()
    {
        startPos = transform.position;
        StartCoroutine(MoveRoutine());
    }

    private System.Collections.IEnumerator MoveRoutine()
    {
        while (true)
        {
            if (!isMoving)
            {
                float randomOffset = Random.Range(-2f, 2f);
                targetPos = new Vector3(startPos.x + randomOffset, startPos.y, startPos.z);
                isMoving = true;
            }

            while (Vector3.Distance(transform.position, targetPos) > 0.05f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
                yield return null;
            }

            yield return new WaitForSeconds(waitTime);

            isMoving = false;
        }
    }
}
