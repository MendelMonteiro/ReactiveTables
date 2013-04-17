using System.Collections.Generic;

namespace ReactiveTables.Framework.Utils
{
    internal static class QueueExtensions
    {
        public static List<T> DequeueAllToList<T>(this Queue<T> queue)
        {
            List<T> elements = new List<T>(queue.Count);
            while (queue.Count > 0)
            {
                elements.Add(queue.Dequeue());
            }
            return elements;
        }
    }
}