# Buff System

버프, 디버프, 상태이상, DoT/HoT 관리 시스템

## 구성 요소

| 클래스 | 설명 |
|--------|------|
| `BuffData` | 버프 정의 ScriptableObject |
| `BuffInstance` | 런타임 버프 인스턴스 |
| `BuffContainer` | 버프 관리 컴포넌트 |
| `BuffStatModifier` | 버프용 스탯 수정자 |
| `StatusEffect` | 상태이상 플래그 열거형 |
| `IBuffable` | 버프 대상 인터페이스 |

## 빠른 시작

### 1. BuffData 생성

Assets > Create > MirrorRPG > Buff > Buff Data

### 2. BuffContainer 추가

```csharp
public class MyEntity : MonoBehaviour, IBuffable
{
    private BuffContainer buffContainer;
    private StatContainer stats;

    public StatContainer Stats => stats;
    public BuffContainer Buffs => buffContainer;

    void Awake()
    {
        buffContainer = new BuffContainer(this);
    }

    void Update()
    {
        buffContainer.Update(Time.deltaTime);
    }
}
```

### 3. 버프 적용

```csharp
// 버프 적용
buffContainer.ApplyBuff(buffData, caster);

// 스택 수 지정 적용
buffContainer.ApplyBuff(buffData, caster, stacks: 3);

// 버프 제거
buffContainer.RemoveBuff(buffId);

// 특정 시전자의 버프 제거
buffContainer.RemoveBuffsFromSource(caster);
```

## BuffData 설정

### 기본 설정

```yaml
Identification:
  buffId: "poison"           # 고유 ID
  displayName: "독"          # UI 표시 이름
  description: "{tickDamage} 데미지를 {tickInterval}초마다 받습니다"
  isDebuff: true             # 디버프 여부

Duration:
  duration: 10               # 지속 시간 (초)
  removeCondition: Time      # 제거 조건
```

### 제거 조건 (RemoveCondition)

| 값 | 설명 |
|---|------|
| `Time` | 시간 경과로 제거 |
| `OnHit` | 피격 시 제거 |
| `OnUseCount` | 사용 횟수 소진 시 제거 |
| `Manual` | 수동으로만 제거 |

### 스택 설정

```yaml
Stacking:
  stackable: true
  maxStacks: 5
  stackBehavior: RefreshDuration
```

| StackBehavior | 설명 |
|---------------|------|
| `RefreshDuration` | 지속 시간 갱신 |
| `AddDuration` | 지속 시간 추가 |
| `Independent` | 독립 인스턴스로 추가 |
| `StackCount` | 스택 수 증가 |

### 스탯 수정자

```yaml
Stat Modifiers:
  - statId: "Attack"
    modifierType: PercentAdd  # Flat, PercentAdd, PercentMult
    value: 0.3                # +30%
    perStack: true            # 스택당 적용
```

### 상태이상 (StatusEffect)

```csharp
[Flags]
public enum StatusEffect
{
    None = 0,
    Stun = 1,       // 행동 불가
    Slow = 2,       // 이동 속도 감소
    Silence = 4,    // 스킬 사용 불가
    Root = 8,       // 이동 불가
    Blind = 16,     // 명중률 감소
    Fear = 32,      // 도주
    Taunt = 64,     // 도발
    Invincible = 128, // 무적
    Invisible = 256   // 투명
}
```

```csharp
// 상태이상 체크
if (buffContainer.HasStatusEffect(StatusEffect.Stun))
{
    // 스턴 상태 처리
}

// 복합 체크
if (buffContainer.HasAnyStatusEffect(StatusEffect.Stun | StatusEffect.Root))
{
    // 이동 불가 처리
}
```

### DoT/HoT (틱 데미지/힐)

```yaml
Tick Effect:
  hasTick: true
  tickInterval: 1.0    # 1초마다
  tickDamage: 10       # 틱당 데미지 (음수면 힐)
  tickDamageType: Fire # 데미지 타입
```

## BuffContainer API

### 버프 조회

```csharp
// 버프 존재 확인
bool hasBuff = buffContainer.HasBuff("rage");

// 버프 인스턴스 가져오기
BuffInstance buff = buffContainer.GetBuff("rage");

// 활성 버프 목록
var activeBuffs = buffContainer.GetActiveBuffs();

// 디버프만 가져오기
var debuffs = buffContainer.GetDebuffs();
```

### 버프 제거

```csharp
// 특정 버프 제거
buffContainer.RemoveBuff("poison");

// 모든 디버프 제거 (최대 N개)
int removed = buffContainer.DispelDebuffs(maxCount: 3);

// 해제 가능한 버프만 제거
buffContainer.RemoveDispellableDebuffs();

// 특정 태그의 버프 제거
buffContainer.RemoveBuffsByTag("fire");
```

### 스택 조작

```csharp
// 스택 추가
buffContainer.AddStacks("rage", 2);

// 스택 제거
buffContainer.RemoveStacks("rage", 1);

// 현재 스택 수
int stacks = buffContainer.GetStackCount("rage");
```

## 시각 효과 (VFX)

BuffData에서 설정:

```yaml
Visual Effects:
  effectPrefab: [활성 중 표시될 이펙트]
  applyEffectPrefab: [적용 시 재생]
  removeEffectPrefab: [제거 시 재생]
  fadeType: Tint      # None, Tint, Outline
  fadeDuration: 0.5
```

## 이벤트

```csharp
buffContainer.OnBuffApplied += (buff) => Debug.Log($"Buff applied: {buff.Data.displayName}");
buffContainer.OnBuffRemoved += (buff) => Debug.Log($"Buff removed: {buff.Data.displayName}");
buffContainer.OnBuffStackChanged += (buff, oldStacks, newStacks) => { };
```

## 설명 템플릿

BuffData의 description에서 플레이스홀더 사용:

```
{duration} - 지속 시간
{stacks} - 현재 스택 수
{tickDamage} - 틱 데미지
{tickInterval} - 틱 간격
{Attack} - 해당 스탯의 수정 값
```

예시: `"{tickDamage} 피해를 {tickInterval}초마다 받습니다. 지속시간: {duration}초"`
