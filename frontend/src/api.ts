export interface Choice {
  id: string;
  text: string;
  targetNodeId: string | null;
}

export interface Node {
  id: string;
  storyId: string;
  text: string;
  mediaUrl?: string;
  choices: Choice[];
}

export interface Story {
  id: string;
  title: string;
  description?: string;
  updatedAt?: string;
}

// Backend JSON Models
interface BackendMedia {
  type: string;
  url: string;
  caption: string;
}

interface BackendChoice {
  id: string;
  text: string;
  next: string;
  media: BackendMedia[];
}

interface BackendNode {
  text: string;
  isEnding: boolean;
  media: BackendMedia[];
  choices: BackendChoice[];
}

interface StoryDefinition {
  startNode: string;
  nodes: Record<string, BackendNode>;
}

export const api = {
  getStories: async (): Promise<Story[]> => {
    const res = await fetch('/api/admin/stories');
    if (!res.ok) return [];
    return await res.json();
  },
  
  createStory: async (title: string, description: string): Promise<Story> => {
    const defaultJson: StoryDefinition = {
      startNode: 'start',
      nodes: {
        'start': { text: 'New Story', isEnding: false, media: [], choices: [] }
      }
    };
    
    const res = await fetch('/api/admin/stories', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ title, description, json: JSON.stringify(defaultJson) })
    });
    return await res.json();
  },

  getStoryNodes: async (storyId: string): Promise<Node[]> => {
    const res = await fetch(`/api/admin/stories/${storyId}`);
    if (!res.ok) return [];
    const story = await res.json();
    
    try {
      const def: StoryDefinition = JSON.parse(story.json);
      const nodes: Node[] = [];
      for (const [key, bNode] of Object.entries(def.nodes)) {
        nodes.push({
          id: key,
          storyId,
          text: bNode.text,
          mediaUrl: bNode.media?.[0]?.url,
          choices: bNode.choices?.map(c => ({
            id: c.id,
            text: c.text,
            targetNodeId: c.next
          })) || []
        });
      }
      return nodes;
    } catch (e) {
      console.error('Failed to parse story JSON', e);
      return [];
    }
  },

  createNode: async (storyId: string, text: string = 'New Node'): Promise<Node> => {
    return {
      id: `node-${Math.random().toString(36).substring(2, 9)}`,
      storyId,
      text,
      choices: []
    };
  },

  // Save the entire story state back to the backend
  saveStory: async (story: Story, nodes: Node[]): Promise<void> => {
    const startNode = nodes.length > 0 ? nodes[0].id : 'start';
    
    const def: StoryDefinition = {
      startNode,
      nodes: {}
    };

    nodes.forEach(n => {
      def.nodes[n.id] = {
        text: n.text,
        isEnding: n.choices.length === 0,
        media: n.mediaUrl ? [{ type: 'image', url: n.mediaUrl, caption: '' }] : [],
        choices: n.choices.map(c => ({
          id: c.id,
          text: c.text,
          next: c.targetNodeId || '',
          media: []
        }))
      };
    });

    await fetch(`/api/admin/stories/${story.id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: story.title,
        description: story.description,
        json: JSON.stringify(def)
      })
    });
  },

  uploadMedia: async (storyId: string, file: File): Promise<string | null> => {
    const formData = new FormData();
    formData.append('file', file);
    const res = await fetch(`/api/admin/stories/${storyId}/media`, {
      method: 'POST',
      body: formData
    });
    if (!res.ok) return null;
    const data = await res.json();
    return data.url;
  }
};
