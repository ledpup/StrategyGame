using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PathFind
{
    public static class PathFind
    {
        public static Path<Node> FindPath<Node>(
            Node start,
            Node destination,
            Func<Node, Node, double> distance,
            Func<Node, double> estimate)
            where Node : IHasNeighbours<Node>
        {
            var closed = new HashSet<Node>();
            var queue = new PriorityQueue<double, Path<Node>>();
            queue.Enqueue(0, new Path<Node>(start));

            while (!queue.IsEmpty)
            {
                var path = queue.Dequeue();

                if (closed.Contains(path.LastStep))
                    continue;
                if (path.LastStep.Equals(destination))
                    return path;

                closed.Add(path.LastStep);

                if (path.LastStep.HasCumulativeCost)
                {
                    path.LastStep.CumulativeCost = 1;
                    if (path.PreviousSteps != null)
                    {
                        foreach (var previous in path.PreviousSteps)
                        {
                            if (previous.HasCumulativeCost)
                            {
                                path.LastStep.CumulativeCost++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                if (path.LastStep.CumulativeCost > 3)
                {
                    continue;
                }

                foreach (Node n in path.LastStep.Neighbours)
                {
                    double d = distance(path.LastStep, n);
                    var newPath = path.AddStep(n, d);
                    queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                }
            }

            return null;
        }
    }
}