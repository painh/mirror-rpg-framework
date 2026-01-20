# Stat System

스탯 정의, 수정자 적용, 리소스 관리를 위한 시스템

## 구성 요소

| 클래스 | 설명 |
|--------|------|
| `Stat` | 기본 스탯 클래스 (공격력, 방어력 등) |
| `ResourceStat` | 리소스 스탯 (HP, MP, 스태미나) |
| `StatModifier` | 스탯 수정자 (버프/장비 효과) |
| `StatDefinition` | 스탯 정의 ScriptableObject |
| `StatSetDefinition` | 스탯 세트 ScriptableObject |
| `StatContainer` | 스탯 관리 컨테이너 |

## 빠른 시작

### 1. StatSetDefinition 생성

Assets > Create > MirrorRPG > Stat > Stat Set Definition

```
PlayerStatSet
├── Health (Resource, 기본값: 100)
├── Stamina (Resource, 기본값: 50)
├── Attack (기본값: 10)
├── Defense (기본값: 5)
└── Speed (기본값: 5)
```

### 2. 코드에서 사용

```csharp
using MirrorRPG.Stat;

public class MyEntity : MonoBehaviour
{
    [SerializeField] private StatSetDefinition statSet;
    private StatContainer stats;

    void Awake()
    {
        stats = new StatContainer(statSet);

        // 이벤트 구독
        stats.OnStatChanged += (statId, stat) => Debug.Log($"{statId} changed to {stat.Value}");
        stats.OnResourceDepleted += (statId, resource) => Debug.Log($"{statId} depleted!");
    }
}
```

## StatContainer API

### 스탯 값 조회

```csharp
// 최종 값 (기본값 + 모든 수정자)
float attack = stats.GetStatValue("Attack");

// 기본값
float baseAttack = stats.GetStatBaseValue("Attack");

// 스탯 객체 직접 접근
Stat attackStat = stats.GetStat("Attack");
```

### 리소스 스탯

```csharp
// 리소스 스탯 조회
ResourceStat health = stats.GetResourceStat("Health");
float current = health.CurrentValue;
float max = health.MaxValue;
float percent = health.Percent;  // 0~1

// 리소스 조작
stats.ReduceResource("Health", 30f);  // 30 감소
stats.RestoreResource("Health", 20f); // 20 회복
stats.FillResource("Health");         // 최대치로 회복

// 헬스 단축 메서드
stats.TakeDamage(50f);  // = ReduceResource("Health", 50f)
stats.Heal(30f);        // = RestoreResource("Health", 30f)
```

### 수정자 적용

```csharp
// 수정자 생성
var flatMod = StatModifier.Flat(10f, source);        // +10
var percentAdd = StatModifier.PercentAdd(0.3f, source);  // +30%
var percentMult = StatModifier.PercentMult(0.5f, source); // x1.5

// 적용
stats.AddModifier("Attack", flatMod);

// 제거
stats.RemoveModifier("Attack", flatMod);

// 특정 소스의 모든 수정자 제거
stats.RemoveModifiersFromSource(buffInstance);

// 전체 수정자 제거
stats.ClearAllModifiers();
```

## 수정자 계산 순서

최종값 = (기본값 + Flat 합계) × (1 + PercentAdd 합계) × (PercentMult 곱)

```
예시: 기본 공격력 100
- Flat +20
- PercentAdd +30% (+50%)
- PercentMult x1.2

최종값 = (100 + 20) × (1 + 0.3 + 0.5) × 1.2 = 259.2
```

## StatModifier Types

| 타입 | 설명 | 예시 |
|------|------|------|
| `Flat` | 고정값 추가 | +50 공격력 |
| `PercentAdd` | 가산 퍼센트 | +30% 공격력 |
| `PercentMult` | 승산 퍼센트 | x1.5 (최종 50% 증가) |

## 내장 스탯 ID

`StatContainer.StatIds`에서 제공하는 상수:

```csharp
StatIds.Health    // "Health"
StatIds.Stamina   // "Stamina"
StatIds.Mana      // "Mana"
StatIds.Attack    // "Attack"
StatIds.Defense   // "Defense"
StatIds.Speed     // "Speed"
StatIds.CritRate  // "CritRate"
StatIds.CritDamage // "CritDamage"
```

## 네트워크 동기화

```csharp
// 서버에서 동기화 데이터 가져오기
var syncData = stats.GetSyncData();

// 클라이언트에서 적용
stats.ApplySyncData(syncData);

// 직접 현재값 설정 (네트워크 동기화용)
stats.SetResourceCurrent("Health", 75f);
```

StatDefinition에서 `networkSync = true` 설정된 스탯만 동기화됩니다.

## 이벤트

| 이벤트 | 발생 시점 |
|--------|----------|
| `OnStatChanged` | 스탯 값 변경 시 |
| `OnResourceChanged` | 리소스 현재값 변경 시 |
| `OnResourceDepleted` | 리소스가 0이 됨 |

```csharp
stats.OnResourceDepleted += (statId, resource) =>
{
    if (statId == StatIds.Health)
    {
        Die();
    }
};
```
