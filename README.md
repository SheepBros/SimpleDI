SimpleDI
=======
## 소개
Unity에서 사용될 수 있는 Dependency Injection 패턴을 구현한 플러그인 입니다. C#으로 구현 되었습니다.   
최대한 단기간에 심플하게 만드는 것을 목표로 한 프로젝트이기 때문에 인터페이스가 좋지 않을 수 있습니다.   

Note: 이 프로젝트는 Unity전용 DI Framework인 [Zenject](https://github.com/modesttree/Zenject)의 Reflection을 이용한 기초적인 알고리즘 아이디어를 얻어 제작되었음을 밝힙니다.

## 사용법
SimpleDI는 제한된 DI 인터페이스를 제공합니다.
SimpleDI를 사용하기 위해서는 Container와 Context, Installer를 알아야합니다.

### Container
Container는 바인드된 인스턴스의 묶음입니다.
부모의 Container를 가지고 있어서(한개 이상일 수 있습니다.), 인스턴스의 참조값을 가져올 때, 부모 Container까지 검색합니다.

### Context
Context란, DiContainer를 멤버로 갖고 있는 클래스로, Inject Attribute를 선언한 인스턴스들에게 의존성 주입을 실행하는 역할을 합니다.
Scene에 종속된 SceneContext와 게임 전체를 아우르는 PersistentContext, 두가지로 분리되어 있습니다.
각 Context는 고유하기 때문에, SceneContext는 Scene당 한개, PersistentContext는 게임에 한개만 존재해야합니다.

#### PersistentContext
PersistentContext는 게임에 고유한 인스턴스를 가진 Context입니다.
모든 SceneContext는 PersistentContext 부모로 두고 있습니다.
기본적으로 SimpleDI는 게임이 시작될 때, Resources폴더의 최상위 위치에서 PersistentContext 라고 명명된 게임오브젝트를 찾습니다.
만약에 PersistentContext 오브젝트가 존재하지 않으면, SimpleDI가 오브젝트를 생성하지만,
전역으로 사용될 인스턴스들을 바인딩 하기 위해선, PersistentContext가 필수이기 때문에 만들어두길 추천합니다.
(기본적으로 SimpleDI에서 PersistentContext를 생성해 놓았습니다.)

### Installer
Installer는 바인딩할 연관된 인스턴스들을 묶는 클래스입니다.
실제 사용자는 Container를 Binding할 때, Installer를 상속한 클래스를 이용하여 바인딩 작업을 수행하게 됩니다.
Installer 컴포넌트로, 기본적으로 Context에게 참조를 전달해야합니다.

### Binding
의존성 주입을 실행 할 인스턴스를 바인딩하기 위해서는 Container의 다음과 같은 함수를 사용하게 됩니다.

**BindAs**
```
Container.BindAs<Type>();
```
가장 기본적인 바인딩 함수이며, 내부적으로 해당 타입으로 인스턴스를 생성해 저장합니다.

**BindTo**
```
Container.BindTo<TFrom, TTo>();
```
TFrom 타입으로 인스턴스를 생성하고, TTo 타입으로 바인딩하게 하는 함수입니다.

**BindAllInterfaces**
```
Container.BindAllInterfaces<Type>();
```
BindAs와 유사하지만, 바인딩 할 때, 해당 타입으로 인스턴스를 생성하고, 바인딩은 해당 타입의 인터페이스들로 실행합니다.
앞으로 나올, IInitializable, IDisposable, IUpdatable을 함께 바인딩할 때 유용합니다.

**BindFrom**
```
Container.BindFrom<Type>(object instance);
```
Scene내에 있는 게임오브젝트 같이, 이미 인스턴스가 존재하는 경우 사용하는 함수입니다.

### Injection
의존성 주입을 사용하기 위해선, 해당 클래스에서 Inject Attribute를 사용해야합니다.
현재 메소드 속성만 제공하며, 사용하기 위해선 다음과 같이 [Inject] Attribute를 넣고 원하는 인스턴스의 타입을 파라미터로 선언합니다.

```
class Installer
{
  public override void InstallBindings()
  {
    Container.BindTo<Service, IService>();
  }
}

class Foo
{
  private IService _service;

  [Inject]
  public void InitInjection(IService service)
  {
    _service = service;
  }
```

### Instantiate & Inject
게임이 진행되는 동안에 동적으로 인스턴스를 생성해서 의존성 주입을 실행해야하는 경우가 있습니다.
그것을 위해서 사용되는 것이 Container의 Instantiate와, Inject 입니다.

```
IService service = Container.Instantiate<Service>() as IService;
```

이미 생성된 인스턴스의 경우 Inject를 호출해 Inject 메소드를 호출 시킬 수 있습니다.
```
IService service = new Service();
Container.Inject(service);
```

### 기타
- SimpleDI는 인스턴스의 의존성 주입에 대하여 초기화 순서를 보장할 수 있도록. 자동으로 Script의 실행순서를 변경합니다.
만약 실행 순서를 변경해야하는 경우가 있다면, Script Execution Order를 확인해주세요.
해당 클래스의 실행순서는 각각 아래와 같이 설정되어 있습니다.
PersistentContext: -9999
SceneContext: -9000
MonoLifeCycle: -8900
만약, SimpleDI와 연관된 인스턴스를 위의 클래스들보다 먼저 실행된다면, 참조 오류를 낼 가능성이 있습니다.
