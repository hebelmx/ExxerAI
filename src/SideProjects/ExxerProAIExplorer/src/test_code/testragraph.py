from ragraph.graph import Graph, Node, Edge

source = Node(name="the source")
target = Node(name="the target")
edge = Edge(source, target)
g = Graph(
    nodes=[source, target],
    edges=[edge],
)
print(g.get_ascii_art())
"""
          ┌───┬───┐
the source┥ ■ │   │
          ├───┼───┤
the target┥ X │ ■ │
          └───┴───┘
"""