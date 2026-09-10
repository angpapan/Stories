import React, { useState, useRef } from 'react';
import { api, Node, Choice } from '../api';
import { Image, Upload, X, Plus } from 'lucide-react';
import ChoiceEditor from './ChoiceEditor';

interface NodeEditorProps {
  nodeId: string;
  allNodes: Node[];
  storyId: string;
  onUpdate: (node: Node) => void;
  onClose: () => void;
  onAddNode: (node: Node) => void;
  onRenameNode: (oldId: string, newId: string) => void;
  onDeleteNode: (nodeId: string) => void;
}

const NodeEditor: React.FC<NodeEditorProps> = ({ nodeId, allNodes, storyId, onUpdate, onClose, onAddNode, onRenameNode, onDeleteNode }) => {
  const [isEditingId, setIsEditingId] = useState(false);
  const [editIdValue, setEditIdValue] = useState('');
  const fileInputRef = useRef<HTMLInputElement>(null);

  const node = allNodes.find(n => n.id === nodeId);

  if (!node) return <div>Loading node...</div>;

  const handleChange = (field: keyof Node, value: any) => {
    const updated = { ...node, [field]: value };
    onUpdate(updated);
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      const url = await api.uploadMedia(storyId, file);
      if (url) {
        handleChange('mediaUrl', url);
      } else {
        alert('File upload failed.');
      }
    }
  };

  const addChoice = () => {
    const newChoice: Choice = {
      id: `c-${Math.random().toString(36).substring(2, 9)}`,
      text: 'New choice',
      targetNodeId: null
    };
    const updated = { ...node, choices: [...node.choices, newChoice] };
    onUpdate(updated);
  };

  const updateChoice = (updatedChoice: Choice) => {
    const updatedChoices = node.choices.map(c => 
      c.id === updatedChoice.id ? updatedChoice : c
    );
    onUpdate({ ...node, choices: updatedChoices });
  };

  const deleteChoice = (choiceId: string) => {
    const updatedChoices = node.choices.filter(c => c.id !== choiceId);
    onUpdate({ ...node, choices: updatedChoices });
  };

  const handleRenameSubmit = () => {
    if (editIdValue.trim() && editIdValue !== node.id) {
      onRenameNode(node.id, editIdValue.trim());
    }
    setIsEditingId(false);
  };

  return (
    <div className="node-editor">
      <div className="editor-header">
        <h3>Edit Node 
          {isEditingId ? (
            <input 
              autoFocus
              className="node-id-input"
              value={editIdValue}
              onChange={e => setEditIdValue(e.target.value)}
              onBlur={handleRenameSubmit}
              onKeyDown={e => {
                if (e.key === 'Enter') handleRenameSubmit();
                if (e.key === 'Escape') setIsEditingId(false);
              }}
            />
          ) : (
            <span 
              className="node-id-badge clickable" 
              onClick={() => {
                setEditIdValue(node.id);
                setIsEditingId(true);
              }}
              title="Click to rename"
            >
              {node.id}
            </span>
          )}
        </h3>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          <button 
            className="btn-secondary btn-small" 
            onClick={() => {
              if (window.confirm('Are you sure you want to delete this node?')) {
                onDeleteNode(node.id);
              }
            }}
            style={{ color: '#ff4d4f', borderColor: '#ff4d4f' }}
            title="Delete Node"
          >
            Delete
          </button>
          <button className="btn-icon mobile-only" onClick={onClose}>
            <X size={20} />
          </button>
        </div>
      </div>

      <div className="form-group">
        <label>Node Text</label>
        <textarea 
          value={node.text}
          onChange={(e) => handleChange('text', e.target.value)}
          rows={4}
          placeholder="Enter the story text for this node..."
        />
      </div>

      <div className="form-group media-group">
        <label>Media Attachment</label>
        <div className="media-input-row">
          <div className="url-input-container">
            <Image size={18} className="input-icon" />
            <input 
              type="text" 
              value={node.mediaUrl || ''}
              onChange={(e) => handleChange('mediaUrl', e.target.value)}
              placeholder="Enter image URL..."
            />
          </div>
          <div className="or-divider">OR</div>
          <button className="btn-secondary" onClick={() => fileInputRef.current?.click()}>
            <Upload size={16} /> Upload File
          </button>
          <input 
            type="file" 
            accept="image/*,video/*"
            ref={fileInputRef}
            style={{ display: 'none' }}
            onChange={handleFileUpload}
          />
        </div>
        {node.mediaUrl && (
          <div className="media-preview">
            <img src={node.mediaUrl} alt="Node media preview" />
            <button className="btn-remove-media" onClick={() => handleChange('mediaUrl', '')}>
              <X size={16} />
            </button>
          </div>
        )}
      </div>

      <div className="choices-section">
        <div className="section-header">
          <h4>Choices</h4>
          <button className="btn-secondary btn-small" onClick={addChoice}>
            <Plus size={16} /> Add Choice
          </button>
        </div>
        
        {node.choices.length === 0 ? (
          <p className="empty-choices">This node has no choices. It might be an end node.</p>
        ) : (
          <div className="choices-list">
            {node.choices.map(choice => (
              <ChoiceEditor 
                key={choice.id} 
                choice={choice} 
                allNodes={allNodes}
                storyId={storyId}
                onChange={updateChoice}
                onDelete={() => deleteChoice(choice.id)}
                onAddNode={onAddNode}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default NodeEditor;
