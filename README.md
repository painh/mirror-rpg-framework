# Combat System Package

Unity용 히트박스/허트박스 기반 전투 시스템 패키지

## 기능

- **Hurtbox 시스템**: 피해를 받는 부위 정의 및 부위별 데미지 배율
- **Hitbox 시스템**: 공격 충돌체 관리 및 다단히트 방지
- **데미지 타입**: 물리, 원소, 특수 속성 지원 (Flags enum)
- **인터페이스 기반**: `IDamageable` 구현으로 모든 엔티티와 연동

## 설치

### Unity Package Manager (Git URL)

1. Window > Package Manager 열기
2. `+` 버튼 클릭 > "Add package from git URL..."
3. 다음 URL 입력:
```
https://github.com/YOUR_USERNAME/com.unity.combat.git
```

### 로컬 개발

```bash
# 패키지를 Packages 폴더에 심볼릭 링크로 연결
ln -s /path/to/com.unity.combat /path/to/your-project/Packages/com.unity.combat
```

## 사용법

### IDamageable 구현

```csharp
using Combat;

public class MyEntity : MonoBehaviour, IDamageable
{
    public GameObject GameObject => gameObject;
    public bool IsAlive => health > 0;

    public void TakeDamage(DamageInfo info)
    {
        health -= info.FinalDamage;
        // 추가 처리...
    }
}
```

### Hurtbox 설정

1. HurtboxData ScriptableObject 생성 (Create > Combat > Hurtbox Data)
2. 부위별 데미지 배율 설정
3. HurtboxManager 컴포넌트를 엔티티에 추가
4. `SetupHurtboxes()` 호출로 자동 생성

### WeaponHitbox 사용

```csharp
// 공격 시작
weaponHitboxController.BeginAttack(damage);

// 공격 종료
weaponHitboxController.EndAttack();

// 스킬 기반 (다단히트 방지)
weaponHitboxController.BeginSkill();
weaponHitboxController.BeginSkillHit(damage, hitboxNames, DamageType.PhysicalSlash);
weaponHitboxController.EndSkillHit(hitboxNames);
weaponHitboxController.EndSkill();
```

## 구조

```
com.unity.combat/
├── Runtime/
│   ├── Core/
│   │   ├── IDamageable.cs      # 데미지 받는 엔티티 인터페이스
│   │   ├── IDamageDealer.cs    # 데미지 주는 오브젝트 인터페이스
│   │   ├── DamageInfo.cs       # 데미지 정보 구조체
│   │   ├── DamageType.cs       # 데미지 속성 (Flags enum)
│   │   └── DamageHelper.cs     # 데미지 처리 유틸리티
│   ├── Hurtbox/
│   │   ├── Hurtbox.cs          # 개별 허트박스
│   │   ├── HurtboxManager.cs   # 허트박스 매니저
│   │   ├── HurtboxData.cs      # 설정 ScriptableObject
│   │   └── HurtboxPartData.cs  # 부위 설정 데이터
│   └── Hitbox/
│       ├── WeaponHitbox.cs           # 무기 히트박스
│       └── WeaponHitboxController.cs # 히트박스 컨트롤러
└── Editor/
    └── (에디터 도구 추가 예정)
```

## 네트워크 연동

`DamageHelper`와 `HurtboxManager`에 콜백을 등록하여 네트워크 처리:

```csharp
// 데미지 처리 전 콜백 (네트워크에서 처리 시 true 반환)
DamageHelper.OnBeforeDamage = (damageable, info) => {
    if (NetworkContext.IsMultiplayer) {
        // 서버에서 처리
        return true;
    }
    return false;
};
```

## 라이센스

MIT
