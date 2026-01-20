# Entity System

엔티티 인터페이스 (이동, 타겟팅)

## 구성 요소

| 인터페이스 | 설명 |
|------------|------|
| `IMovable` | 이동 가능한 엔티티 |
| `ITargetable` | 타겟팅 가능한 엔티티 |

## IMovable

이동 관련 속성과 메서드 정의

```csharp
using MirrorRPG.Entity;

public interface IMovable
{
    /// <summary>
    /// 현재 이동 속도
    /// </summary>
    float CurrentSpeed { get; }

    /// <summary>
    /// 최대 이동 속도
    /// </summary>
    float MaxSpeed { get; }

    /// <summary>
    /// 이동 가능 여부
    /// </summary>
    bool CanMove { get; }

    /// <summary>
    /// 현재 이동 방향 (정규화됨)
    /// </summary>
    Vector3 MoveDirection { get; }

    /// <summary>
    /// 특정 위치로 이동
    /// </summary>
    void MoveTo(Vector3 position);

    /// <summary>
    /// 이동 중지
    /// </summary>
    void StopMovement();
}
```

### 구현 예시

```csharp
public class MyCharacter : MonoBehaviour, IMovable
{
    [SerializeField] private float maxSpeed = 5f;
    private CharacterController controller;
    private Vector3 moveDirection;
    private bool canMove = true;

    public float CurrentSpeed => controller.velocity.magnitude;
    public float MaxSpeed => maxSpeed;
    public bool CanMove => canMove;
    public Vector3 MoveDirection => moveDirection;

    public void MoveTo(Vector3 position)
    {
        if (!canMove) return;
        moveDirection = (position - transform.position).normalized;
    }

    public void StopMovement()
    {
        moveDirection = Vector3.zero;
    }

    void Update()
    {
        if (moveDirection != Vector3.zero)
        {
            controller.Move(moveDirection * maxSpeed * Time.deltaTime);
        }
    }
}
```

## ITargetable

타겟팅 관련 속성 정의

```csharp
using MirrorRPG.Entity;

public interface ITargetable
{
    /// <summary>
    /// 타겟으로 지정 가능 여부
    /// </summary>
    bool IsTargetable { get; }

    /// <summary>
    /// 타겟팅 중심점 (UI 표시 위치)
    /// </summary>
    Vector3 TargetPoint { get; }

    /// <summary>
    /// 엔티티의 팀/진영
    /// </summary>
    int TeamId { get; }

    /// <summary>
    /// 생존 여부
    /// </summary>
    bool IsAlive { get; }
}
```

### 구현 예시

```csharp
public class MyEnemy : MonoBehaviour, ITargetable
{
    [SerializeField] private Transform targetPoint;
    [SerializeField] private int teamId = 1; // 적 팀
    private float health = 100f;

    public bool IsTargetable => IsAlive;
    public Vector3 TargetPoint => targetPoint != null ? targetPoint.position : transform.position + Vector3.up;
    public int TeamId => teamId;
    public bool IsAlive => health > 0;
}
```

## 통합 사용

여러 인터페이스를 조합하여 완전한 엔티티 구현:

```csharp
using Combat;
using MirrorRPG.Entity;
using MirrorRPG.Stat;
using MirrorRPG.Buff;

public class GameEntity : MonoBehaviour, IMovable, ITargetable, IDamageable, IBuffable
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 5f;
    private CharacterController controller;
    private Vector3 moveDirection;
    private bool canMove = true;

    [Header("Targeting")]
    [SerializeField] private Transform targetPoint;
    [SerializeField] private int teamId;

    [Header("Stats")]
    [SerializeField] private StatSetDefinition statSet;
    private StatContainer stats;
    private BuffContainer buffs;

    // IMovable
    public float CurrentSpeed => controller.velocity.magnitude;
    public float MaxSpeed => maxSpeed;
    public bool CanMove => canMove && !buffs.HasStatusEffect(StatusEffect.Root | StatusEffect.Stun);
    public Vector3 MoveDirection => moveDirection;

    // ITargetable
    public bool IsTargetable => IsAlive;
    public Vector3 TargetPoint => targetPoint.position;
    public int TeamId => teamId;
    public bool IsAlive => stats.CurrentHealth > 0;

    // IDamageable
    public GameObject GameObject => gameObject;

    // IBuffable
    public StatContainer Stats => stats;
    public BuffContainer Buffs => buffs;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        stats = new StatContainer(statSet);
        buffs = new BuffContainer(this);
    }

    void Update()
    {
        buffs.Update(Time.deltaTime);
        UpdateMovement();
    }

    public void MoveTo(Vector3 position)
    {
        if (!CanMove) return;
        moveDirection = (position - transform.position).normalized;
    }

    public void StopMovement()
    {
        moveDirection = Vector3.zero;
    }

    public void TakeDamage(DamageInfo info)
    {
        stats.TakeDamage(info.FinalDamage);

        if (!IsAlive)
        {
            Die();
        }
    }

    private void UpdateMovement()
    {
        if (!CanMove || moveDirection == Vector3.zero) return;

        // 슬로우 상태 체크
        float speedMult = buffs.HasStatusEffect(StatusEffect.Slow) ? 0.5f : 1f;
        controller.Move(moveDirection * maxSpeed * speedMult * Time.deltaTime);
    }

    private void Die()
    {
        StopMovement();
        // 사망 처리
    }
}
```

## 유틸리티: 타겟 찾기

```csharp
public static class TargetFinder
{
    /// <summary>
    /// 범위 내 가장 가까운 적 찾기
    /// </summary>
    public static ITargetable FindNearestEnemy(Vector3 position, float range, int myTeamId)
    {
        ITargetable nearest = null;
        float nearestDist = float.MaxValue;

        var colliders = Physics.OverlapSphere(position, range);
        foreach (var col in colliders)
        {
            var target = col.GetComponent<ITargetable>();
            if (target == null) continue;
            if (!target.IsTargetable) continue;
            if (target.TeamId == myTeamId) continue; // 같은 팀 제외

            float dist = Vector3.Distance(position, target.TargetPoint);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = target;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 범위 내 모든 적 찾기
    /// </summary>
    public static List<ITargetable> FindEnemiesInRange(Vector3 position, float range, int myTeamId)
    {
        var enemies = new List<ITargetable>();

        var colliders = Physics.OverlapSphere(position, range);
        foreach (var col in colliders)
        {
            var target = col.GetComponent<ITargetable>();
            if (target == null) continue;
            if (!target.IsTargetable) continue;
            if (target.TeamId == myTeamId) continue;

            enemies.Add(target);
        }

        return enemies;
    }

    /// <summary>
    /// 시야 내 적 찾기 (각도 제한)
    /// </summary>
    public static List<ITargetable> FindEnemiesInCone(
        Vector3 position,
        Vector3 forward,
        float range,
        float angle,
        int myTeamId)
    {
        var enemies = new List<ITargetable>();
        float halfAngle = angle / 2f;

        var colliders = Physics.OverlapSphere(position, range);
        foreach (var col in colliders)
        {
            var target = col.GetComponent<ITargetable>();
            if (target == null) continue;
            if (!target.IsTargetable) continue;
            if (target.TeamId == myTeamId) continue;

            Vector3 dirToTarget = (target.TargetPoint - position).normalized;
            float angleToTarget = Vector3.Angle(forward, dirToTarget);

            if (angleToTarget <= halfAngle)
            {
                enemies.Add(target);
            }
        }

        return enemies;
    }
}
```
