## TODO 

~~- 좌표 체계가 혼동스러움. 일단 이거 정리하자.~~
~~- Connector 구현 하기~~  
- 새로운 UI 구현.  
- Connector 테스트 하기, Dispose 관련해서 테스트 진행하기.  
- Connector 구현 후 PendingConnection 완성.  
~~- Node UI 초기 설계, Node 이동 구현 필요.(NodeEditor 확인)~~  
- Node 이동 시 최종적인 위치 변경은 위치 변경 후 Invallid~ 호출 그리고 데이터 저장해야 함. 이 구현은 거의 마지막에 이루어 질듯.   
~~- Panning 구현 다시 정리하기.(DAGlynEditor 구현 및 정리)~~   
~~- Panning 관련하여 Canvas 의 사이즈가 고정되서 움직여서 이것에 대한 버그가 발생한다. 이것을 해결해야 한다.~~  
- DirectProperty 좀더 연구.  
- Nodify 에서 Node 추가하는 소스 코드 한번 살펴본다.  
- Connector 연결 구현, 이동에 따른 연결점(Anchor) 의 업데이트 구현, PendingConnection 및 연결 완료되었을때 연결선 구현  
~~- Connector 연결 구현을 위해서 Connector 에서의 PendingConnectionEventArgs 이벤트 발생시키고, 핸들러를 DAGlynEditor 추가해서 처리해야함.~~  
~~- PendingConnectionEventArgs 코드 정리 및 수정해야함.~~  
- 위의 연결 끝내고 Dispose 처리 해야겠다. 계속 신경쓰인다. Dispose 테스트 진행할때, Unload 관련 해서 좀더 깊게 들어가 본다.
~~- default 값으로 new 해주는 값인 경우, 이걸 별도로 static 으로 만들어 놓는 거와 새롭게 new 해주는 거랑 어떤게 성능이 더 좋은지 파악해야 할 거 같다.~~  
~~- PendingConnection 을 Editor 의 axaml 에 넣는 방향으로 하는게 코드량이 줄어들 것 같다. PendingConnection 만들어주는 부분~~    
~~- 여러 Layer 잡아주는 것 구현.~~    

## 코드 정리 순서

## 벤치마킹
- Pro .NET Benchmarking 책 읽고 싶다. T.T
- https://github.com/dotnet/BenchmarkDotNet
