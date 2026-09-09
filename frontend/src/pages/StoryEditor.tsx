import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api, Story, Node } from '../api';
import NodeEditor from '../components/NodeEditor';
import { ArrowLeft, Plus } from 'lucide-react';

const StoryEditor: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [story, setStory] = useState<Story | null>(null);
  const [nodes, setNodes] = useState<Node[]>([]);
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);

  useEffect(() => {
    if (id) {
      loadData(id);
    }
  }, [id]);

  const loadData = async (storyId: string) => {
    const stories = await api.getStories();
    const st = stories.find(s => s.id === storyId);
    if (st) setStory(st);
    
    const nds = await api.getStoryNodes(storyId);
    setNodes(nds);
  };

  const handleCreateNode = async () => {
    if (!id) return;
    const newNode = await api.createNode(id, 'New blank node');
    setNodes([...nodes, newNode]);
    setSelectedNodeId(newNode.id);
  };

  const handleNodeUpdate = (updatedNode: Node) => {
    setNodes(prev => prev.map(n => n.id === updatedNode.id ? updatedNode : n));
  };

  const handleAddNode = (newNode: Node) => {
    setNodes(prev => [...prev, newNode]);
  };

  const handleRenameNode = (oldId: string, newId: string) => {
    if (oldId === newId) return;
    if (newId.trim() === '') {
      alert('Node ID cannot be empty');
      return;
    }
    
    setNodes(prev => {
      if (prev.some(n => n.id === newId)) {
        alert('A node with this ID already exists!');
        return prev;
      }
      return prev.map(n => {
        if (n.id === oldId) {
          const renamed = { ...n, id: newId };
          renamed.choices = renamed.choices.map(c => 
            c.targetNodeId === oldId ? { ...c, targetNodeId: newId } : c
          );
          return renamed;
        }
        const updatedChoices = n.choices.map(c => 
          c.targetNodeId === oldId ? { ...c, targetNodeId: newId } : c
        );
        return { ...n, choices: updatedChoices };
      });
    });
    
    if (selectedNodeId === oldId) {
      setSelectedNodeId(newId);
    }
  };

  const handleSave = async () => {
    if (!story) return;
    try {
      await api.saveStory(story, nodes);
      alert('Story saved successfully!');
    } catch (e) {
      alert('Failed to save story.');
      console.error(e);
    }
  };

  if (!story) return <div className="loading">Loading story...</div>;

  return (
    <div className="story-editor-container">
      <div className="story-editor-header">
        <Link to="/" className="back-link">
          <ArrowLeft size={18} /> Back to Stories
        </Link>
        <div className="title-section">
          <h2>{story.title}</h2>
          <p>{story.description}</p>
        </div>
        <div className="header-actions">
          <button className="btn btn-primary" onClick={handleSave}>Save Story</button>
        </div>
      </div>

      <div className="story-layout">
        <div className={`nodes-sidebar ${selectedNodeId ? 'hide-on-mobile' : ''}`}>
          <div className="sidebar-header">
            <h3>Nodes</h3>
            <button className="btn-icon" onClick={handleCreateNode} title="Add Node">
              <Plus size={20} />
            </button>
          </div>
          <ul className="nodes-list">
            {nodes.map(node => (
              <li 
                key={node.id} 
                className={`node-list-item ${selectedNodeId === node.id ? 'active' : ''}`}
                onClick={() => setSelectedNodeId(node.id)}
              >
                <div className="node-id-label">{node.id}</div>
                <div className="node-text-preview">
                  {node.text ? node.text.substring(0, 30) + (node.text.length > 30 ? '...' : '') : 'Empty Node'}
                </div>
              </li>
            ))}
            {nodes.length === 0 && (
              <li className="empty-state">No nodes yet. Create one!</li>
            )}
          </ul>
        </div>

        <div className={`node-editor-area ${!selectedNodeId ? 'hide-on-mobile' : ''}`}>
          {selectedNodeId ? (
            <NodeEditor 
              nodeId={selectedNodeId} 
              allNodes={nodes} 
              onUpdate={handleNodeUpdate}
              onClose={() => setSelectedNodeId(null)}
              storyId={story.id}
              onAddNode={handleAddNode}
              onRenameNode={handleRenameNode}
            />
          ) : (
            <div className="empty-editor">
              <p>Select a node from the sidebar to edit it.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default StoryEditor;
