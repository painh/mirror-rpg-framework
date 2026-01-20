# Skill System

스킬 정의, 액션 타이밍, 실행 관리 시스템

## 구성 요소

| 클래스 | 설명 |
|--------|------|
| `ISkillData` | 스킬 데이터 인터페이스 |
| `SkillData` | 스킬 정의 ScriptableObject |
| `SkillAction` | 스킬 액션 기본 클래스 |
| `DurationSkillAction` | 지속형 액션 기본 클래스 |
| `SkillExecutor` | 스킬 실행 관리 컴포넌트 |
| `SkillActionContext` | 액션 실행 컨텍스트 |

## 빠른 시작

### 1. SkillData 생성

Assets > Create > MirrorRPG > Skill > Skill Data

### 2. SkillExecutor 사용

```csharp
public class MyCharacter : MonoBehaviour
{
    [SerializeField] private SkillData fireball;
    private SkillExecutor executor;

    void Awake()
    {
        executor = gameObject.AddComponent<SkillExecutor>();
        executor.OnSkillEnded += OnSkillComplete;
    }

    public void UseSkill()
    {
        executor.ExecuteSkill(fireball, target);
    }

    private void OnSkillComplete(ISkillData skill)
    {
        Debug.Log($"Skill finished: {skill.SkillName}");
    }
}
```

## SkillData 설정

### 기본 정보

```yaml
Basic Info:
  skillName: "파이어볼"
  description: "화염구를 발사합니다"
  icon: [스킬 아이콘]

Animation:
  animationTrigger: "Throw"
  animationSpeed: 1.0
  duration: 1.2    # 스킬 총 지속 시간
```

### 전투 효과

```yaml
Combat:
  combatEffect: [CombatEffect SO 참조]
  cooldown: 3.0
  range: 15.0
  angle: 60       # 근접 스킬 범위 각도
```

### 옵션

```yaml
Options:
  canMoveWhileCasting: false   # 시전 중 이동
  canRotateWhileCasting: true  # 시전 중 회전
  hasSuperArmor: false         # 슈퍼아머
  resourceCost: 20             # 마나/스태미나 소모
```

## Skill Actions

스킬 실행 중 특정 타이밍에 발생하는 액션들

### 내장 액션 종류

| 액션 | 설명 |
|------|------|
| `HitboxAction` | 히트박스 활성화 (지속형) |
| `SpawnProjectileAction` | 투사체 생성 |
| `SpawnVFXAction` | 이펙트 생성 |
| `PlaySoundAction` | 사운드 재생 |
| `ApplyBuffAction` | 버프 적용 |

### HitboxAction (지속형)

```yaml
startTime: 0.3      # 히트박스 활성화 시점
endTime: 0.5        # 히트박스 비활성화 시점
hitboxIndex: 0      # 히트박스 인덱스
```

### SpawnProjectileAction

```yaml
startTime: 0.4                    # 발사 시점
projectilePrefabPath: "Projectile"  # Resources 경로
projectileDataPath: "ProjectileData/Fireball"
spawnOffset: (0, 1, 0.5)         # 스폰 위치 오프셋
aimAtTarget: true                 # 타겟 조준
count: 1                          # 투사체 수
spreadAngle: 15                   # 다중 투사체 확산 각도
```

### SpawnVFXAction

```yaml
startTime: 0.2
vfxPrefabPath: "VFX/FireCharge"
attachToOwner: true
duration: 1.5
```

### PlaySoundAction

```yaml
startTime: 0.0
soundClipPath: "Sounds/FireballCast"
volume: 1.0
```

### ApplyBuffAction

```yaml
startTime: 0.5
buffDataPath: "Buffs/SpeedBoost"
applyToSelf: true
duration: 5.0
```

## SkillExecutor API

### 스킬 실행

```csharp
// 기본 실행
executor.ExecuteSkill(skillData, target);

// 실행 중 확인
bool isActive = executor.IsExecuting;
ISkillData current = executor.CurrentSkill;

// 스킬 취소
executor.CancelSkill();
```

### 스폰 포인트 설정

```csharp
// 투사체/이펙트 스폰 위치
executor.SpawnPoint = handTransform;
```

### 이벤트

```csharp
executor.OnSkillStarted += (skill) => { };
executor.OnSkillEnded += (skill) => { };
executor.OnSkillCancelled += (skill) => { };
executor.OnActionTriggered += (action) => { };
```

## SkillActionContext

액션에 전달되는 컨텍스트 정보

```csharp
public class SkillActionContext
{
    public GameObject Owner;      // 시전자
    public ISkillData SkillData;  // 스킬 데이터
    public Transform SpawnPoint;  // 스폰 위치
    public GameObject Target;     // 타겟
    public Vector3 Direction;     // 방향
    public Dictionary<string, object> CustomData;
}
```

## 커스텀 액션 만들기

### 인스턴트 액션

```csharp
using MirrorRPG.Skill;

[System.Serializable]
public class KnockbackAction : SkillAction
{
    public float force = 10f;
    public float upwardForce = 2f;

    public override void Execute(SkillActionContext context)
    {
        if (context.Target == null) return;

        var rb = context.Target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (context.Target.transform.position - context.Owner.transform.position).normalized;
            dir.y = 0;
            rb.AddForce(dir * force + Vector3.up * upwardForce, ForceMode.Impulse);
        }
    }
}
```

### 지속형 액션

```csharp
[System.Serializable]
public class SlowMotionAction : DurationSkillAction
{
    public float timeScale = 0.5f;
    private float originalTimeScale;

    public override void OnStart(SkillActionContext context)
    {
        originalTimeScale = Time.timeScale;
        Time.timeScale = timeScale;
    }

    public override void OnEnd(SkillActionContext context)
    {
        Time.timeScale = originalTimeScale;
    }

    public override void OnCancel(SkillActionContext context)
    {
        Time.timeScale = originalTimeScale;
    }
}
```

## ISkillData 인터페이스

직접 구현하여 커스텀 스킬 데이터 만들기:

```csharp
public interface ISkillData
{
    string SkillName { get; }
    float Duration { get; }
    float BaseDamage { get; }
    DamageType DamageTypes { get; }
    IReadOnlyList<SkillHitTiming> HitTimings { get; }
    IReadOnlyList<SkillAction> Actions { get; }
    CombatEffect CombatEffect { get; }
    string AnimationTrigger { get; }
    float Cooldown { get; }
}
```

## 네트워크 동기화

멀티플레이어에서 투사체 스킬 처리:

```csharp
// 클라이언트: 스킬 실행은 로컬, 투사체는 서버 명령
if (NetworkContext.IsMultiplayer && hasProjectileAction)
{
    // SkillExecutor 대신 수동 타이머 사용
    // 타이밍에 맞춰 CmdSpawnProjectile 호출
}
else
{
    // 싱글플레이어: 기존 방식
    skillExecutor.ExecuteSkill(skill, target);
}
```
