import React from 'react';
import { api, Choice, Node } from '../api';
import { Trash2, Link as LinkIcon, PlusCircle } from 'lucide-react';

interface ChoiceEditorProps {
  choice: Choice;
  allNodes: Node[];
  storyId: string;
  onChange: (choice: Choice) => void;
  onDelete: () => void;
  onAddNode: (node: Node) => void;
}

const ChoiceEditor: React.FC<ChoiceEditorProps> = ({ 
  choice, allNodes, storyId, onChange, onDelete, onAddNode 
}) => {

  const handleTextChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange({ ...choice, text: e.target.value });
  };

  const handleTargetChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const val = e.target.value;
    onChange({ ...choice, targetNodeId: val === 'null' ? null : val });
  };

  const createAndLinkNewNode = async () => {
    const newNode = await api.createNode(storyId, 'New continued node');
    onChange({ ...choice, targetNodeId: newNode.id });
    onAddNode(newNode);
  };

  return (
    <div className="choice-editor-card">
      <div className="choice-header">
        <input 
          className="choice-text-input"
          type="text" 
          value={choice.text}
          onChange={handleTextChange}
          placeholder="Choice text (e.g., 'Open the door')"
        />
        <button className="btn-icon danger" onClick={onDelete} title="Delete choice">
          <Trash2 size={16} />
        </button>
      </div>

      <div className="choice-target-row">
        <div className="target-select-wrapper">
          <LinkIcon size={16} className="input-icon" />
          <select 
            value={choice.targetNodeId || 'null'} 
            onChange={handleTargetChange}
            className="target-select"
          >
            <option value="null">-- Select target node --</option>
            {allNodes.map(n => (
              <option key={n.id} value={n.id}>
                [{n.id}] {n.text ? n.text.substring(0, 30) + (n.text.length > 30 ? '...' : '') : 'Empty node'}
              </option>
            ))}
          </select>
        </div>
        <div className="or-divider">OR</div>
        <button className="btn-secondary btn-small" onClick={createAndLinkNewNode}>
          <PlusCircle size={14} /> New Node
        </button>
      </div>
    </div>
  );
};

export default ChoiceEditor;
