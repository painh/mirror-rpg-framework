# Mirror RPG Framework

Unity용 RPG 게임 개발을 위한 재사용 가능한 핵심 시스템 패키지

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

## 시스템 목차

| 시스템 | 설명 | 문서 |
|--------|------|------|
| **Stat** | 스탯 정의, 수정자, 리소스 관리 | [stat-system.md](docs/stat-system.md) |
| **Buff** | 버프/디버프, 상태이상, DoT/HoT | [buff-system.md](docs/buff-system.md) |
| **Skill** | 스킬 정의, 액션 타이밍, 실행 | [skill-system.md](docs/skill-system.md) |
| **Combat** | 데미지 처리, 히트박스/허트박스 | [combat-system.md](docs/combat-system.md) |
| **StateMachine** | 범용 상태 머신, 애니메이션 연동 | [statemachine-system.md](docs/statemachine-system.md) |
| **Entity** | 이동, 타겟팅 인터페이스 | [entity-system.md](docs/entity-system.md) |
| **EventGraph** | 비주얼 이벤트/대화 그래프 시스템 | - |

## 빠른 시작

### 1. 기본 엔티티 설정

```csharp
using MirrorRPG.Stat;
using MirrorRPG.Buff;
using Combat;

public class GameEntity : MonoBehaviour, IDamageable, IBuffable
{
    [SerializeField] private StatSetDefinition statSet;

    private StatContainer stats;
    private BuffContainer buffs;

    public StatContainer Stats => stats;
    public BuffContainer Buffs => buffs;
    public GameObject GameObject => gameObject;
    public bool IsAlive => stats.CurrentHealth > 0;

    void Awake()
    {
        stats = new StatContainer(statSet);
        buffs = new BuffContainer(this);
    }

    void Update()
    {
        buffs.Update(Time.deltaTime);
    }

    public void TakeDamage(DamageInfo info)
    {
        stats.TakeDamage(info.FinalDamage);
    }
}
```

### 2. 스킬 사용

```csharp
using MirrorRPG.Skill;

public class MyCharacter : MonoBehaviour
{
    [SerializeField] private SkillData fireballSkill;
    private SkillExecutor skillExecutor;

    void Awake()
    {
        skillExecutor = gameObject.AddComponent<SkillExecutor>();
    }

    public void UseFireball(GameObject target)
    {
        skillExecutor.ExecuteSkill(fireballSkill, target);
    }
}
```

### 3. 상태 머신

```csharp
using MirrorRPG.StateMachine;

public class IdleState : StateBase<PlayerStateMachine>
{
    public override void Enter() { /* 상태 진입 */ }
    public override void Update() { /* 상태 업데이트 */ }
    public override void Exit() { /* 상태 종료 */ }
}
```

## 폴더 구조

```
com.unity.mirror-rpg/
├── Runtime/
│   ├── Buff/          # 버프 시스템
│   ├── Combat/        # 전투 효과
│   ├── Core/          # 데미지 시스템
│   ├── Entity/        # 엔티티 인터페이스
│   ├── EventGraph/    # 이벤트 그래프 시스템
│   ├── Hitbox/        # 히트박스
│   ├── Hurtbox/       # 허트박스
│   ├── Skill/         # 스킬 시스템
│   ├── Stat/          # 스탯 시스템
│   └── StateMachine/  # 상태 머신
├── Editor/
└── docs/              # 상세 문서
```

## 네임스페이스

| 네임스페이스 | 설명 |
|--------------|------|
| `MirrorRPG.Stat` | 스탯 시스템 |
| `MirrorRPG.Buff` | 버프 시스템 |
| `MirrorRPG.Skill` | 스킬 시스템 |
| `MirrorRPG.Combat` | 전투 효과 |
| `MirrorRPG.Entity` | 엔티티 인터페이스 |
| `MirrorRPG.StateMachine` | 상태 머신 |
| `MirrorRPG.EventGraph` | 이벤트 그래프 코어 |
| `MirrorRPG.EventGraph.Nodes` | 이벤트 노드 타입 |
| `Combat` | 데미지 코어 |

## 주요 ScriptableObject

| 타입 | 생성 경로 |
|------|----------|
| `StatDefinition` | MirrorRPG > Stat > Stat Definition |
| `StatSetDefinition` | MirrorRPG > Stat > Stat Set Definition |
| `BuffData` | MirrorRPG > Buff > Buff Data |
| `SkillData` | MirrorRPG > Skill > Skill Data |
| `CombatEffect` | MirrorRPG > Combat > Combat Effect |
| `EventGraphAsset` | MirrorRPG > Event Graph |

## 라이센스

MIT
