using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float baseSpeed = 5f;  // 基础速度

    Vector3 offset;  // 相机与玩家之间的偏移量
    
    void Update()
    {
        MoveCamera();
    }

    void MoveCamera()
    {
        float distance = Vector3.Distance(new Vector3(transform.position.x, transform.position.y, 0), new Vector3(target.position.x, target.position.y, 0));
        if (distance > 5f)
        {
            transform.position = new Vector3(target.position.x, target.position.y, -1);
        }
        float speed = baseSpeed + distance * 0.2f; // 计算速度，基础速度加上距离的一半

        // 移动相机位置到玩家位置，速度随距离增加而增加，同时固定Z坐标为-1
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, -1); // 设置目标位置的Z为-1
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }
}
