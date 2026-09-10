import React, { useEffect, useState } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import { api, Story, Node } from '../api';
import NodeEditor from '../components/NodeEditor';
import { ArrowLeft, Plus, Eye, EyeOff, Copy } from 'lucide-react';

const StoryEditor: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [story, setStory] = useState<Story | null>(null);
  const [nodes, setNodes] = useState<Node[]>([]);
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  const [isEditingTitle, setIsEditingTitle] = useState(false);
  const [isEditingDesc, setIsEditingDesc] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [toastMsg, setToastMsg] = useState<string | null>(null);

  const showToast = (msg: string) => {
    setToastMsg(msg);
    setTimeout(() => setToastMsg(null), 2500);
  };

  useEffect(() => {
    if (id) {
      loadData(id);
    }
  }, [id]);

  useEffect(() => {
    if (story) {
      document.title = story.title ? `${story.title} - Editor` : 'Story Editor';
    } else {
      document.title = 'Story Editor';
    }
  }, [story]);

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

  const handleDeleteNode = (nodeId: string) => {
    setNodes(prev => prev.filter(n => n.id !== nodeId).map(n => ({
      ...n,
      choices: n.choices.map(c => c.targetNodeId === nodeId ? { ...c, targetNodeId: null } : c)
    })));
    if (selectedNodeId === nodeId) {
      setSelectedNodeId(null);
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
        <Link to="/admin" className="back-link">
          <ArrowLeft size={18} /> Back to Stories
        </Link>
        <div className="title-section" style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', flex: 1, padding: '0 1rem' }}>
          {isEditingTitle ? (
            <input
              autoFocus
              type="text"
              value={story.title}
              onChange={(e) => setStory({ ...story, title: e.target.value })}
              onBlur={() => setIsEditingTitle(false)}
              onKeyDown={(e) => {
                if (e.key === 'Enter' || e.key === 'Escape') setIsEditingTitle(false);
              }}
              style={{ fontSize: '1.5rem', fontWeight: 'bold', border: '1px solid #444', borderRadius: '4px', padding: '0.25rem', background: 'transparent', color: 'inherit' }}
              placeholder="Story Title"
            />
          ) : (
            <h2
              onClick={() => setIsEditingTitle(true)}
              style={{ cursor: 'text', margin: 0 }}
              title="Click to edit title"
            >
              {story.title || 'Untitled Story'}
            </h2>
          )}

          {isEditingDesc ? (
            <textarea
              autoFocus
              value={story.description || ''}
              onChange={(e) => setStory({ ...story, description: e.target.value })}
              onBlur={() => setIsEditingDesc(false)}
              onKeyDown={(e) => {
                if (e.key === 'Escape') setIsEditingDesc(false);
              }}
              style={{ fontFamily: 'inherit', border: '1px solid #444', borderRadius: '4px', padding: '0.25rem', resize: 'vertical', minHeight: '60px', background: 'transparent', color: 'inherit' }}
              placeholder="Story Description"
            />
          ) : (
            <p
              onClick={() => setIsEditingDesc(true)}
              style={{ cursor: 'text', margin: 0, minHeight: '1.5em', color: story.description ? 'inherit' : '#888' }}
              title="Click to edit description"
            >
              {story.description || 'No description provided. Click to add one.'}
            </p>
          )}

          <div 
            className="story-id" 
            style={{ fontSize: '0.75rem', color: '#666', display: 'flex', alignItems: 'center', gap: '0.5rem', marginTop: '0.5rem' }}
            onClick={(e) => {
              e.preventDefault();
              navigator.clipboard.writeText(story.id);
              showToast('ID copied to clipboard!');
            }}
            title="Click to copy ID"
          >
            <span>ID: {story.id}</span>
            <Copy size={12} style={{ cursor: 'pointer' }} />
          </div>

          <div className="story-settings" style={{ display: 'flex', gap: '1rem', marginTop: '0.5rem', alignItems: 'center', flexWrap: 'wrap' }}>
            <div className="setting-item" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <label htmlFor="password" style={{ fontSize: '0.875rem', color: '#888' }}>Password:</label>
              <div style={{ display: 'flex', alignItems: 'center', position: 'relative' }}>
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  value={story.password || ''}
                  onChange={(e) => setStory({ ...story, password: e.target.value })}
                  placeholder="Leave empty for public"
                  style={{ background: 'transparent', border: '1px solid #444', borderRadius: '4px', padding: '0.25rem', color: 'inherit', fontSize: '0.875rem', paddingRight: '2rem' }}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  style={{ position: 'absolute', right: '0.25rem', background: 'none', border: 'none', color: '#888', cursor: 'pointer', display: 'flex', alignItems: 'center', padding: 0 }}
                  title={showPassword ? "Hide password" : "Show password"}
                >
                  {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>
            <div className="setting-item" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <label htmlFor="passwordHint" style={{ fontSize: '0.875rem', color: '#888' }}>Password Hint:</label>
              <input
                id="passwordHint"
                type="text"
                value={story.passwordHint || ''}
                onChange={(e) => setStory({ ...story, passwordHint: e.target.value })}
                placeholder="Optional hint"
                style={{ background: 'transparent', border: '1px solid #444', borderRadius: '4px', padding: '0.25rem', color: 'inherit', fontSize: '0.875rem' }}
              />
            </div>
            <div className="setting-item" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <label htmlFor="maxPlaythroughs" style={{ fontSize: '0.875rem', color: '#888' }}>Max Playthroughs:</label>
              <input
                id="maxPlaythroughs"
                type="number"
                min="0"
                value={story.maxPlaythroughs || ''}
                onChange={(e) => setStory({ ...story, maxPlaythroughs: e.target.value ? parseInt(e.target.value, 10) : undefined })}
                placeholder="Unlimited"
                style={{ background: 'transparent', border: '1px solid #444', borderRadius: '4px', padding: '0.25rem', color: 'inherit', width: '100px', fontSize: '0.875rem' }}
              />
            </div>
          </div>
        </div>
        <div className="header-actions" style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', paddingTop: '1rem' }}>
          <button className="btn btn-secondary" onClick={() => navigate(`/admin/story/${story.id}/playthroughs`)}>
            <Eye size={16} /> View Playthroughs
          </button>
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
              onDeleteNode={handleDeleteNode}
            />
          ) : (
            <div className="empty-editor">
              <p>Select a node from the sidebar to edit it.</p>
            </div>
          )}
        </div>
      </div>
      
      {toastMsg && <div className="elegant-toast">{toastMsg}</div>}
    </div>
  );
};

export default StoryEditor;
