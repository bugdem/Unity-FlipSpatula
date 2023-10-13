
namespace ClocknestGames.Library.Utils
{
    public enum SceneEventType : byte
    {
        Unloaded = 1
    }

    public struct SceneEvent
    {
        public SceneEventType EventType;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneEvent"/> struct.
        /// </summary>
        /// <param name="eventType">Type of level event.</param>
        public SceneEvent(SceneEventType eventType)
        {
            EventType = eventType;
        }
    }

    public class SceneEventListener : Singleton<SceneEventListener>
    {

    }
}