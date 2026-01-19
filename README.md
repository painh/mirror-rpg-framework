# Mirror RPG Framework

Unity용 RPG 게임 개발을 위한 재사용 가능한 핵심 시스템 패키지

## 기능

- **Combat 시스템**: 히트박스/허트박스 기반 전투, 데미지 타입 및 처리
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
│   │   ├── IDamageable.cs       # 데미지 받는 엔티티 인터페이스
│   │   ├── IDamageDealer.cs     # 데미지 주는 오브젝트 인터페이스
│   │   ├── DamageInfo.cs        # 데미지 정보 구조체
│   │   ├── DamageType.cs        # 데미지 속성 (Flags enum)
│   │   └── DamageHelper.cs      # 데미지 처리 유틸리티
│   ├── Entity/                  # 엔티티 인터페이스
│   │   ├── IMovable.cs          # 이동 인터페이스
│   │   └── ITargetable.cs       # 타겟팅/인지/공격 인터페이스
│   ├── Hurtbox/                 # 허트박스 시스템
│   │   ├── Hurtbox.cs           # 개별 허트박스
│   │   ├── HurtboxManager.cs    # 허트박스 매니저
│   │   └── HurtboxData.cs       # 설정 ScriptableObject
│   ├── Hitbox/                  # 히트박스 시스템
│   │   ├── WeaponHitbox.cs      # 무기 히트박스
│   │   └── WeaponHitboxController.cs
│   ├── Skill/                   # 스킬 시스템
│   │   ├── ISkillData.cs        # 스킬 데이터 인터페이스
│   │   ├── SkillHitTiming.cs    # 히트 타이밍 정의
│   │   └── SkillTimingHelper.cs # 타이밍 유틸리티
│   └── StateMachine/            # 상태 머신
│       ├── IStateMachineOwner.cs    # 소유자 인터페이스
│       ├── IAnimationController.cs  # 애니메이션 인터페이스
│       ├── AnimatorAdapter.cs       # Animator 어댑터
│       ├── StateMachineBase.cs      # 상태 머신 베이스
│       └── StateBase.cs             # 상태 베이스
└── Editor/
    └── (에디터 도구 추가 예정)
```

## 네임스페이스

- `Combat` - 데미지 시스템
- `MirrorRPG.Entity` - 엔티티 인터페이스
- `MirrorRPG.Skill` - 스킬 시스템
- `MirrorRPG.StateMachine` - 상태 머신

## 라이센스

MIT
