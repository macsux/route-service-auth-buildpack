using Moq;
using Pivotal.RouteService.Auth.Ingress.Buildpack;
using Xunit;

namespace UnitTests
{
    public class GenericProcessorTests
    {
        Mock<IDetector> detector;
        Mock<IConfigFileAppender> configAppender;
        Mock<IAssemblyMover> assemMover;

        public GenericProcessorTests()
        {
            detector = new Mock<IDetector>();
            configAppender = new Mock<IConfigFileAppender>();
            assemMover = new Mock<IAssemblyMover>();
        }

        [Fact]
        public void Test_DoesNothingIfDetectorReturnsFalse()
        {
            detector.Setup(d => d.Find()).Returns(false);

            var processor = new GenericProcessor(detector.Object, configAppender.Object, assemMover.Object);

            processor.Execute();

            configAppender.Verify(c => c.Execute(), Times.Never);
            assemMover.Verify(a => a.Move(), Times.Never);
        }

        [Fact]
        public void Test_ExecutesEachExecutorsIfDetectorReturnsTrue()
        {
            detector.Setup(d => d.Find()).Returns(true);

            var processor = new GenericProcessor(detector.Object, configAppender.Object, assemMover.Object);

            processor.Execute();

            configAppender.Verify(c => c.Execute(), Times.Once);
            assemMover.Verify(a => a.Move(), Times.Once);
        }
    }
}
