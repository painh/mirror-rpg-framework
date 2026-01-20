# StateMachine System

범용 상태 머신 및 애니메이션 연동 시스템

## 구성 요소

| 클래스 | 설명 |
|--------|------|
| `StateMachineBase<T>` | 상태 머신 기본 클래스 |
| `StateBase<T>` | 상태 기본 클래스 |
| `IStateMachineOwner` | 상태 머신 소유자 인터페이스 |
| `IAnimationController` | 애니메이션 컨트롤러 인터페이스 |
| `AnimatorAdapter` | Unity Animator 어댑터 |

## 빠른 시작

### 1. 상태 머신 정의

```csharp
using MirrorRPG.StateMachine;

public class PlayerStateMachine : StateMachineBase<PlayerStateMachine>
{
    // 상태 인스턴스
    public IdleState IdleState { get; private set; }
    public RunState RunState { get; private set; }
    public AttackState AttackState { get; private set; }

    public PlayerStateMachine(IStateMachineOwner owner) : base(owner)
    {
        IdleState = new IdleState(this);
        RunState = new RunState(this);
        AttackState = new AttackState(this);
    }

    protected override StateBase<PlayerStateMachine> GetInitialState()
    {
        return IdleState;
    }
}
```

### 2. 상태 정의

```csharp
public class IdleState : StateBase<PlayerStateMachine>
{
    public IdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        // 상태 진입 시 처리
        Owner.Animator.SetBool("IsMoving", false);
    }

    public override void Update()
    {
        // 매 프레임 처리
        if (Owner.InputDirection.magnitude > 0.1f)
        {
            StateMachine.ChangeState(StateMachine.RunState);
        }
    }

    public override void Exit()
    {
        // 상태 종료 시 처리
    }
}
```

### 3. 소유자 구현

```csharp
public class PlayerController : MonoBehaviour, IStateMachineOwner
{
    private PlayerStateMachine stateMachine;
    private Animator animator;

    public Animator Animator => animator;
    public Vector3 InputDirection { get; private set; }

    void Awake()
    {
        animator = GetComponent<Animator>();
        stateMachine = new PlayerStateMachine(this);
    }

    void Start()
    {
        stateMachine.Initialize();
    }

    void Update()
    {
        InputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        stateMachine.Update();
    }
}
```

## StateMachineBase API

### 상태 전환

```csharp
// 상태 전환
stateMachine.ChangeState(stateMachine.RunState);

// 현재 상태 확인
var current = stateMachine.CurrentState;

// 이전 상태 확인
var previous = stateMachine.PreviousState;

// 상태 이름
string stateName = stateMachine.CurrentStateName;
```

### 초기화 및 업데이트

```csharp
// 초기화 (Start에서 호출)
stateMachine.Initialize();

// 업데이트 (Update에서 호출)
stateMachine.Update();

// 고정 업데이트 (FixedUpdate에서 호출)
stateMachine.FixedUpdate();
```

## StateBase API

### 생명주기 메서드

```csharp
public class MyState : StateBase<MyStateMachine>
{
    public override void Enter()
    {
        // 상태 진입 시 1회 호출
    }

    public override void Update()
    {
        // 매 프레임 호출
    }

    public override void FixedUpdate()
    {
        // 물리 업데이트 시 호출
    }

    public override void Exit()
    {
        // 상태 종료 시 1회 호출
    }
}
```

### 유틸리티

```csharp
// 소유자 접근
var player = Owner as PlayerController;

// 상태 경과 시간
float elapsed = StateTime;

// 애니메이션 컨트롤러 접근
AnimationController.SetTrigger("Attack");
```

## IStateMachineOwner

상태 머신 소유자가 구현해야 하는 인터페이스

```csharp
public interface IStateMachineOwner
{
    // 필요한 속성과 메서드를 프로젝트에 맞게 정의
}
```

일반적인 구현:

```csharp
public interface IStateMachineOwner
{
    Animator Animator { get; }
    Transform Transform { get; }
    bool IsGrounded { get; }
    Vector3 InputDirection { get; }
    float CurrentSpeed { get; }
}
```

## IAnimationController

애니메이션 제어 추상화 인터페이스

```csharp
public interface IAnimationController
{
    void SetTrigger(string name);
    void SetBool(string name, bool value);
    void SetFloat(string name, float value);
    void SetInteger(string name, int value);
    void ResetTrigger(string name);
    float GetCurrentAnimationTime();
    bool IsInTransition();
}
```

## AnimatorAdapter

Unity Animator를 IAnimationController로 래핑

```csharp
public class PlayerController : MonoBehaviour
{
    private IAnimationController animController;

    void Awake()
    {
        var animator = GetComponent<Animator>();
        animController = new AnimatorAdapter(animator);
    }
}
```

## 상태 전환 패턴

### 조건부 전환

```csharp
public override void Update()
{
    // 입력 기반 전환
    if (Input.GetButtonDown("Attack"))
    {
        StateMachine.ChangeState(StateMachine.AttackState);
        return;
    }

    // 조건 기반 전환
    if (!Owner.IsGrounded)
    {
        StateMachine.ChangeState(StateMachine.FallState);
        return;
    }
}
```

### 시간 기반 전환

```csharp
public override void Update()
{
    // 일정 시간 후 전환
    if (StateTime >= attackDuration)
    {
        StateMachine.ChangeState(StateMachine.IdleState);
    }
}
```

### 애니메이션 기반 전환

```csharp
public override void Update()
{
    // 애니메이션 종료 후 전환
    if (AnimationController.GetCurrentAnimationTime() >= 0.9f)
    {
        StateMachine.ChangeState(StateMachine.IdleState);
    }
}
```

## 상태 데이터 전달

상태 간 데이터 전달이 필요한 경우:

```csharp
public class AttackState : StateBase<PlayerStateMachine>
{
    public ISkillData CurrentSkill { get; private set; }

    public void SetSkill(ISkillData skill)
    {
        CurrentSkill = skill;
    }

    public override void Enter()
    {
        // CurrentSkill 사용
        Owner.Animator.SetTrigger(CurrentSkill.AnimationTrigger);
    }

    public override void Exit()
    {
        CurrentSkill = null;
    }
}

// 사용
var attackState = stateMachine.AttackState;
attackState.SetSkill(mySkill);
stateMachine.ChangeState(attackState);
```

## 네트워크 동기화

멀티플레이어에서 상태 동기화:

```csharp
// SyncVar로 상태 이름 동기화
[SyncVar(hook = nameof(OnStateChanged))]
private string syncedStateName;

private void OnStateChanged(string oldState, string newState)
{
    // 로컬 플레이어가 아닌 경우 상태 적용
    if (!isLocalPlayer)
    {
        ApplyRemoteState(newState);
    }
}

// 상태 변경 시 서버에 알림
[Command]
private void CmdChangeState(string stateName)
{
    syncedStateName = stateName;
}
```
