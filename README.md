# Mirror RPG Framework

Unity용 RPG 게임 개발을 위한 재사용 가능한 핵심 시스템 패키지

## 기능

- **Combat 시스템**: 히트박스/허트박스 기반 전투, 데미지 타입 및 처리
- **Stat 시스템**: 스탯 정의, 수정자, 리소스 스탯 (HP/MP/스태미나)
- **Buff 시스템**: 버프/디버프, 상태이상, DoT/HoT, 스택 관리
- **Entity 시스템**: 이동, 타겟팅, 인지, 공격 인터페이스
- **StateMachine 시스템**: 범용 상태 머신 및 애니메이션 연동
- **Skill 시스템**: 스킬 정의 및 다단히트 타이밍 관리

## 설치

### Unity Package Manager (Git URL)

1. Window > Package Manager 열기
2. `+` 버튼 클릭 > "Add package from git URL..."
3. 다음 URL 입력:
```
https://github.com/painh/mirror-rpg-framework.git
```

### 로컬 개발

```bash
# 패키지를 Packages 폴더에 심볼릭 링크로 연결
ln -s /path/to/mirror-rpg-framework /path/to/your-project/Packages/com.unity.mirror-rpg
```

## 시스템 개요

### Stat 시스템

스탯 정의 및 수정자 기반 계산

```csharp
using MirrorRPG.Stat;

// StatContainer로 스탯 관리
var container = GetComponent<StatContainer>();
float attack = container.GetStatValue("Attack");

// 수정자 적용 (버프 등에서)
var modifier = StatModifier.PercentAdd(0.3f, source); // +30%
container.AddModifier("Attack", modifier);

// 리소스 스탯 (HP, MP 등)
container.TakeDamage(50f);
container.Heal(30f);
float healthPercent = container.HealthPercent;
```

### Buff 시스템

버프/디버프 및 상태이상 관리

```csharp
using MirrorRPG.Buff;

// 버프 적용
var buffHandler = GetComponent<BuffHandler>();
buffHandler.ApplyBuff(poisonBuffData, attacker);

// 상태이상 체크
if (buffHandler.HasStatusEffect(StatusEffect.Stun))
{
    // 스턴 상태 처리
}

// 디버프 해제
buffHandler.DispelDebuffs(maxCount: 2);
```

**BuffData SO 설정:**
- 스탯 수정 (Flat, PercentAdd, PercentMult)
- 상태이상 (Stun, Slow, Silence, Root 등)
- DoT/HoT (틱 데미지/힐)
- 스택 정책 (RefreshDuration, AddDuration, Independent)
- 해제 조건 (Time, OnHit, OnUseCount)

### Combat (Core, Hitbox, Hurtbox)

데미지 처리 및 충돌 시스템

```csharp
using Combat;

public class MyEntity : MonoBehaviour, IDamageable
{
    public GameObject GameObject => gameObject;
    public bool IsAlive => health > 0;

    public void TakeDamage(DamageInfo info)
    {
        health -= info.FinalDamage;
    }
}
```

### Entity 시스템

이동 및 타겟팅 인터페이스

```csharp
using MirrorRPG.Entity;

public class MyCharacter : MonoBehaviour, IMovable, ITargetable, IAttacker
{
    // IMovable: 이동 속도, 방향, 걷기/달리기
    // ITargetable: 타겟 설정, 감지 범위, 공격 범위
    // IAttacker: 공격 범위, 데미지, 쿨다운
}
```

### StateMachine 시스템

범용 상태 머신

```csharp
using MirrorRPG.StateMachine;

public class IdleState : StateBase<MyStateMachine>
{
    public override void Enter() { /* 상태 진입 */ }
    public override void Update() { /* 상태 업데이트 */ }
    public override void Exit() { /* 상태 종료 */ }
}
```

### Skill 시스템

스킬 정의 및 타이밍 관리

```csharp
using MirrorRPG.Skill;

[CreateAssetMenu]
public class MySkillData : ScriptableObject, ISkillData
{
    public string SkillName => skillName;
    public float Duration => duration;
    public IReadOnlyList<SkillHitTiming> HitTimings => hitTimings;
    // ...
}
```

## 구조

```
com.unity.mirror-rpg/
├── Runtime/
│   ├── Core/                    # 데미지 시스템
│   │   ├── IDamageable.cs
│   │   ├── IDamageDealer.cs
│   │   ├── DamageInfo.cs
│   │   ├── DamageType.cs
│   │   └── DamageHelper.cs
│   ├── Stat/                    # 스탯 시스템
│   │   ├── Stat.cs              # 기본 스탯 클래스
│   │   ├── ResourceStat.cs      # 리소스 스탯 (HP/MP)
│   │   ├── StatModifier.cs      # 스탯 수정자
│   │   ├── StatDefinition.cs    # 스탯 정의 SO
│   │   ├── StatSetDefinition.cs # 스탯 세트 SO
│   │   └── StatContainer.cs     # 스탯 관리 컴포넌트
│   ├── Buff/                    # 버프 시스템
│   │   ├── BuffData.cs          # 버프 정의 SO
│   │   ├── BuffInstance.cs      # 런타임 버프 인스턴스
│   │   ├── BuffHandler.cs       # 버프 관리 컴포넌트
│   │   ├── IBuffable.cs         # 버프 대상 인터페이스
│   │   ├── StatusEffect.cs      # 상태이상 Flags enum
│   │   └── BuffStatModifier.cs  # 버프용 스탯 수정자
│   ├── Entity/                  # 엔티티 인터페이스
│   │   ├── IMovable.cs
│   │   └── ITargetable.cs
│   ├── Hurtbox/                 # 허트박스 시스템
│   │   ├── Hurtbox.cs
│   │   ├── HurtboxManager.cs
│   │   └── HurtboxData.cs
│   ├── Hitbox/                  # 히트박스 시스템
│   │   ├── WeaponHitbox.cs
│   │   └── WeaponHitboxController.cs
│   ├── Skill/                   # 스킬 시스템
│   │   ├── ISkillData.cs
│   │   ├── SkillHitTiming.cs
│   │   └── SkillTimingHelper.cs
│   └── StateMachine/            # 상태 머신
│       ├── IStateMachineOwner.cs
│       ├── IAnimationController.cs
│       ├── AnimatorAdapter.cs
│       ├── StateMachineBase.cs
│       └── StateBase.cs
└── Editor/
    └── (에디터 도구 추가 예정)
```

## 네임스페이스

- `Combat` - 데미지 시스템
- `MirrorRPG.Stat` - 스탯 시스템
- `MirrorRPG.Buff` - 버프/디버프 시스템
- `MirrorRPG.Entity` - 엔티티 인터페이스
- `MirrorRPG.Skill` - 스킬 시스템
- `MirrorRPG.StateMachine` - 상태 머신

## 라이센스

MIT
