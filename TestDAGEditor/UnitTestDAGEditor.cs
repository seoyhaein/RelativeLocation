using RelativeLocation;
using Xunit.Abstractions;
using JetBrains.dotMemoryUnit;

namespace TestDAGEditor;

/*
 * 아래 테스트 코드는 일단 잘못되었다.
 * 하지만, 일단 이런형식으로 진행한다.
 * Dispose 테스트는 다른 형식으로 진행했다.
 */
// unit 테스트와 dotMemoryUnit 를 혼용해서 사용하기 위해서.
[DotMemoryUnit(FailIfRunWithoutSupport = false)]
public class UnitTestDAGEditor
{
    private readonly ITestOutputHelper output;

    public UnitTestDAGEditor(ITestOutputHelper output)
    {
        this.output = output;
        DotMemoryUnitTestOutput.SetOutputMethod(output.WriteLine);
    }
    
    [Fact]
    public void TestDoDispose()
    {
        // Arrange
        var editor = new DAGlynEditor();
        //bool isDisposed = false;

        //editor.Unloaded += (_, __) => isDisposed = true;

        // Act
        //Extension.LogPerformance("Dispose 사용 전");
        dotMemory.Check(memory =>
        {
            // Dispose 호출 전의 메모리 상태 스냅샷
            Assert.True(memory.GetObjects(where => where.Type.Is<DAGlynEditor>()).ObjectsCount == 1);
        });
        
        editor.Dispose();
        
        dotMemory.Check(memory =>
        {
            // Dispose 호출 후의 메모리 상태 스냅샷
            // DAGlynEditor 인스턴스가 메모리에서 제거되었는지 검증
            Assert.True(memory.GetObjects(where => where.Type.Is<DAGlynEditor>()).ObjectsCount == 1);
        });
        //Extension.LogPerformance("Dispose 사용 후");

        // Assert
        //Assert.True(isDisposed, "Dispose 메서드가 정상적으로 호출되지 않았습니다.");
    }
}