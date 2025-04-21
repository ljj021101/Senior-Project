using UnityEngine;

public class EnemyBattleController : MonoBehaviour
{
    [Header("组件引用")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    [Header("不同敌人资源")]
    public Sprite slimeSprite;
    public RuntimeAnimatorController slimeAnimator;

    public Sprite goblinSprite;
    public RuntimeAnimatorController goblinAnimator;

    public Sprite batSprite;
    public RuntimeAnimatorController batAnimator;

    public Sprite mimicSprite;
    public RuntimeAnimatorController mimicAnimator;

    public void Setup(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Slime:
                spriteRenderer.sprite = slimeSprite;
                animator.runtimeAnimatorController = slimeAnimator;
                break;
            case EnemyType.Goblin:
                spriteRenderer.sprite = goblinSprite;
                animator.runtimeAnimatorController = goblinAnimator;
                break;
            case EnemyType.Bat:
                spriteRenderer.sprite = batSprite;
                animator.runtimeAnimatorController = batAnimator;
                break;
            case EnemyType.Mimic:
                spriteRenderer.sprite = mimicSprite;
                animator.runtimeAnimatorController = mimicAnimator;
                break;
        }
    }
}
