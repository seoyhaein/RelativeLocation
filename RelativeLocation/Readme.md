## TODO 

~~- 좌표 체계가 혼동스러움. 일단 이거 정리하자.~~
~~- Connector 구현 하기~~  
- 새로운 UI 구현.  
~~- Connector 테스트 하기, Dispose 관련해서 테스트 진행하기.~~    
~~- Connector 구현 후 PendingConnection 완성.~~    
~~- Node UI 초기 설계, Node 이동 구현 필요.(NodeEditor 확인)~~  
- Node 이동 시 최종적인 위치 변경은 위치 변경 후 Invallid~ 호출 그리고 데이터 저장해야 함. 이 구현은 거의 마지막에 이루어 질듯.   
~~- Panning 구현 다시 정리하기.(DAGlynEditor 구현 및 정리)~~   
~~- Panning 관련하여 Canvas 의 사이즈가 고정되서 움직여서 이것에 대한 버그가 발생한다. 이것을 해결해야 한다.~~  
- DirectProperty 좀더 연구.  
~~- Nodify 에서 Node 추가하는 소스 코드 한번 살펴본다.~~  
~~- Connector 연결 구현, 이동에 따른 연결점(Anchor) 의 업데이트 구현, PendingConnection 및 연결 완료되었을때 연결선 구현~~    
~~- Connector 연결 구현을 위해서 Connector 에서의 PendingConnectionEventArgs 이벤트 발생시키고, 핸들러를 DAGlynEditor 추가해서 처리해야함.~~  
~~- PendingConnectionEventArgs 코드 정리 및 수정해야함.~~  
~~- 위의 연결 끝내고 Dispose 처리 해야겠다. 계속 신경쓰인다. Dispose 테스트 진행할때,~~   
- Unload 관련 해서 좀더 깊게 들어가 본다.
~~- default 값으로 new 해주는 값인 경우, 이걸 별도로 static 으로 만들어 놓는 거와 새롭게 new 해주는 거랑 어떤게 성능이 더 좋은지 파악해야 할 거 같다.~~  
~~- PendingConnection 을 Editor 의 axaml 에 넣는 방향으로 하는게 코드량이 줄어들 것 같다. PendingConnection 만들어주는 부분~~    
~~- 여러 Layer 잡아주는 것 구현.~~    
- PendingConnection UI 개선
~~- Connection 연결선 Layer 만들어 주기 및 속성 추가. Connection 연결선 생성 루틴 개발~~    
~~- unit 테스트 에서 Dispse 관련 테스트 진행 중 dotMemory 등 사용방법 숙지가 필요 할듯하다. 계속 실폐 중~~
~~- 이벤트에 따라 Connection 추가하기~~  
~~- Panning 시 Connection 의 잔상이 남는 버그 처리해야함.~~ 
~~- 버그 수정~~  
~~- Node 에서 Anchor 2 개가 설정되어야 한다.(In/Out) 이게 Connector 의 Anchor 에 바인딩 하는 형태로 가야 한다.~~    
~~- StartNode 는 Anchor(Out) 하나 있고, EndNode 도 Anchor(In) 하나 있다.~~ 
~~- 대략적으로 완성했지만 버그 있음. 버그는 내일 고치자 (2/29) Unloaded 처리 부분도 생각하자.~~    
~~- Node 위치 변경 시, 최종적으로 해당 위치를 업데이트는 하는 부분을 넣어두고 이것을 토대로 Anchor 계산하는 코드를 적용하자.~~
- 위의 내용 구현 후 테스트 좀 잘하고 github 업데이트 하자. 된장.   
~~- 새로운 버그 Node 의 특정 위치에서 이동이 이상하게 나타남. 그리고 "invalid transform string" 라는 이상이 나타남.~~    
~~- Canvas 와 EditorCanvas, LayoutCanvas 의 Bound 를 살펴보기로 한다. 이거 정검한번 해야함. 일단 버그는 여기서 발생한 문제는 아니다.~~  
~~- 하지만, 위의 코드는 한번 살펴보고 Bound 문제등도 한번 정검해보자. 시간날때. 일단 지우지 않고 남겨둔다.~~  
~~- Avalonia.Controls.Primitives 에서 https://reference.avaloniaui.net/api/Avalonia.Controls.Primitives/Thumb/BE3BA1F0~~
~~- 위의 코드 살펴보고, node 의 drag 를 개선하자. 현재 마우스 고속으로 움직였을때 화면 튀는 현상 발생한다.~~  
- 여러 노드를 추가 하는 구문을 넣어야 함.    
~~- 노드의 삭제~~   
- connection 의 삭제를 넣어야 함.  
- Connection 을 고도화 해야 함. 지금은 그냥 선만 넣어둔 형태임.  
- 노드를 생성하기 위한 UI 를 생각하자. 메뉴에 관한 스터디 및 어떻게 구현할지 고민해야 함.    
- 리소스 정리하자.  
~~- 1 차적으로 에디터에 배경 집어 넣는 것을 완성했는데, 추가적인 모양들을 넣어야 할 것 같다.~~  
- Rider 에서 UI 보여주는 화면과 비슷하게 만들자. (시간날때, 기능상으로는 핵심 기능은 아님)
- node 가 focus 를 가질때 ui 변경 (구별해주어야 함.)  
- compiled binding 적용 중 (잊어버리고 있었음.)
- https://docs.avaloniaui.net/docs/basics/data/data-binding/compiled-bindings

## 버그
- Panning 한다음에 선을 연결 시키면 이상하게 나옴.  
- 노드 이동시 잔상 남는 현상 해결 해야 함.  
- 선 연결시 미세한 버그 있음.  

## 지속적으로 TODO 정리하자.  
## 벤치마킹
- Pro .NET Benchmarking 책 읽고 싶다. T.T
- https://github.com/dotnet/BenchmarkDotNet
