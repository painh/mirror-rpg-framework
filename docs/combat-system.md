# Combat System

데미지 처리, 히트박스/허트박스, 전투 효과 시스템

## 구성 요소

### Core (데미지 처리)

| 클래스 | 설명 |
|--------|------|
| `IDamageable` | 데미지를 받을 수 있는 대상 |
| `IDamageDealer` | 데미지를 줄 수 있는 대상 |
| `DamageInfo` | 데미지 정보 구조체 |
| `DamageType` | 데미지 타입 플래그 |
| `DamageHelper` | 데미지 계산 헬퍼 |

### Combat (전투 효과)

| 클래스 | 설명 |
|--------|------|
| `CombatEffect` | 전투 효과 ScriptableObject |
| `TargetAffinity` | 타겟 친화도 |
| `BuffApplication` | 버프 적용 정보 |

### Hitbox/Hurtbox

| 클래스 | 설명 |
|--------|------|
| `WeaponHitbox` | 무기 히트박스 |
| `WeaponHitboxController` | 히트박스 컨트롤러 |
| `Hurtbox` | 허트박스 컴포넌트 |
| `HurtboxManager` | 허트박스 관리자 |
| `HurtboxData` | 허트박스 설정 데이터 |

## IDamageable 구현

```csharp
using Combat;

public class MyEntity : MonoBehaviour, IDamageable
{
    [SerializeField] private float health = 100f;

    public GameObject GameObject => gameObject;
    public bool IsAlive => health > 0;

    public void TakeDamage(DamageInfo info)
    {
        health -= info.FinalDamage;

        // 데미지 표시, 히트 리액션 등
        ShowDamageNumber(info.FinalDamage, info.IsCritical);

        if (health <= 0)
        {
            Die();
        }
    }
}
```

## DamageInfo

```csharp
public struct DamageInfo
{
    public float BaseDamage;      // 기본 데미지
    public float FinalDamage;     // 최종 데미지
    public DamageType DamageTypes; // 데미지 타입
    public bool IsCritical;       // 크리티컬 여부
    public Vector3 HitPoint;      // 피격 위치
    public Vector3 HitDirection;  // 피격 방향
    public GameObject Attacker;   // 공격자
    public GameObject Weapon;     // 무기
    public HurtboxPartType HitPart; // 피격 부위
}
```

## DamageType

```csharp
[Flags]
public enum DamageType
{
    None = 0,
    Physical = 1,     // 물리
    Fire = 2,         // 화염
    Ice = 4,          // 냉기
    Lightning = 8,    // 번개
    Poison = 16,      // 독
    Holy = 32,        // 신성
    Dark = 64,        // 암흑
    True = 128        // 고정 (방어 무시)
}

// 복합 타입
DamageType firePhysical = DamageType.Physical | DamageType.Fire;

// 타입 체크
if ((damageType & DamageType.Fire) != 0)
{
    // 화염 데미지 포함
}
```

## CombatEffect 설정

Assets > Create > MirrorRPG > Combat > Combat Effect

### 기본 설정

```yaml
Basic Info:
  effectName: "파이어볼 폭발"
  description: "화염 데미지를 입힙니다"

Targeting:
  targetAffinity: Enemy    # Enemy, Ally, Self, All

Effect Type:
  effectType: Damage       # Damage, Heal, DamageAndHeal
```

### 데미지/힐

```yaml
Damage / Heal:
  baseValue: 50            # 기본 값
  valueMultiplier: 1.0     # 배율
  damageType: Fire         # 데미지 타입
  canCritical: true        # 크리티컬 가능
```

### 버프 적용

```yaml
Buff/Debuff Application:
  - buffData: [BurnDebuff]
    chance: 0.5            # 50% 확률
    stacks: 1
```

### 이펙트

```yaml
VFX/SFX:
  hitVFX: [폭발 이펙트]
  hitVFXDuration: 2.0
  hitSound: [폭발 사운드]
```

## TargetAffinity

| 값 | 설명 |
|---|------|
| `Enemy` | 적만 영향 |
| `Ally` | 아군만 영향 |
| `Self` | 자신만 영향 |
| `All` | 모두 영향 |

```csharp
// CombatEffect에서 타겟 검증
bool canAffect = combatEffect.CanAffect(isSameTeam, isSelf);
```

## WeaponHitbox

근접 무기 충돌 감지

```csharp
public class MyWeapon : MonoBehaviour
{
    private WeaponHitbox hitbox;

    void Awake()
    {
        hitbox = GetComponent<WeaponHitbox>();
        hitbox.OnHit += OnWeaponHit;
    }

    void Attack()
    {
        hitbox.Activate(damage: 50f, duration: 0.3f);
    }

    private void OnWeaponHit(IDamageable target, DamageInfo info)
    {
        // 히트 처리
    }
}
```

## WeaponHitboxController

여러 히트박스 관리

```csharp
public class MyCharacter : MonoBehaviour
{
    private WeaponHitboxController hitboxController;

    void Awake()
    {
        hitboxController = GetComponent<WeaponHitboxController>();
    }

    void PerformComboAttack(int hitIndex)
    {
        // 인덱스로 히트박스 활성화
        hitboxController.ActivateHitbox(hitIndex, damage: 30f, duration: 0.2f);
    }

    void HeavyAttack()
    {
        // 이름으로 히트박스 활성화
        hitboxController.ActivateHitbox("heavy", damage: 80f, duration: 0.4f);
    }
}
```

## Hurtbox System

피격 영역 정의

### HurtboxManager 설정

```csharp
public class MyEntity : MonoBehaviour
{
    private HurtboxManager hurtboxManager;

    void Awake()
    {
        hurtboxManager = GetComponent<HurtboxManager>();
        hurtboxManager.OnHurtboxHit += OnPartHit;
    }

    private void OnPartHit(Hurtbox hurtbox, DamageInfo info)
    {
        // 부위별 데미지 배율 적용
        float multiplier = hurtbox.DamageMultiplier;
        float finalDamage = info.BaseDamage * multiplier;

        // 헤드샷 등 특수 처리
        if (hurtbox.PartType == HurtboxPartType.Head)
        {
            // 크리티컬 보장 등
        }
    }
}
```

### HurtboxPartType

```csharp
public enum HurtboxPartType
{
    Body,       // 몸통
    Head,       // 머리
    LeftArm,    // 왼팔
    RightArm,   // 오른팔
    LeftLeg,    // 왼다리
    RightLeg,   // 오른다리
    WeakPoint,  // 약점
    Armor       // 방어구
}
```

## DamageHelper

데미지 계산 유틸리티

```csharp
using Combat;

// 데미지 계산
DamageInfo info = DamageHelper.CalculateDamage(
    baseDamage: 100f,
    attacker: attackerStats,
    defender: defenderStats,
    damageType: DamageType.Physical,
    canCrit: true
);

// 방어력 적용
float reducedDamage = DamageHelper.ApplyDefense(damage, defense);

// 크리티컬 판정
bool isCrit = DamageHelper.RollCritical(critRate);
float critDamage = DamageHelper.ApplyCritical(damage, critDamageMultiplier);
```
