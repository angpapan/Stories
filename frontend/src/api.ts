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
  password?: string;
  passwordHint?: string;
  maxPlaythroughs?: number;
  updatedAt?: string;
}

export interface PlaythroughHistory {
  nodeId: string;
  chosenChoiceId?: string;
  answeredAt: string;
  sequenceNumber: number;
}

export interface Playthrough {
  id: string;
  status: number; // 0 = InProgress, 1 = Completed
  createdAt: string;
  completedAt?: string;
  currentNodeId: string;
  history: PlaythroughHistory[];
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

const fetchAdmin = (url: string, init?: RequestInit) => {
  return fetch(url, { ...init, credentials: 'include' });
};

export const api = {
  login: async (password: string): Promise<boolean> => {
    const res = await fetchAdmin('/api/admin/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ password })
    });
    return res.ok;
  },

  logout: async (): Promise<void> => {
    await fetchAdmin('/api/admin/logout', { method: 'POST' });
  },
  getStories: async (): Promise<Story[]> => {
    const res = await fetchAdmin('/api/admin/stories');
    if (res.status === 401) throw new Error('Unauthorized');
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
    
    const res = await fetchAdmin('/api/admin/stories', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ title, description, json: JSON.stringify(defaultJson) })
    });
    return await res.json();
  },

  getStoryNodes: async (storyId: string): Promise<Node[]> => {
    const res = await fetchAdmin(`/api/admin/stories/${storyId}`);
    if (res.status === 401) throw new Error('Unauthorized');
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

    await fetchAdmin(`/api/admin/stories/${story.id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        title: story.title,
        description: story.description,
        password: story.password || null,
        passwordHint: story.passwordHint || null,
        maxPlaythroughs: story.maxPlaythroughs || null,
        json: JSON.stringify(def)
      })
    });
  },

  uploadMedia: async (storyId: string, file: File): Promise<string | null> => {
    const formData = new FormData();
    formData.append('file', file);
    const res = await fetchAdmin(`/api/admin/stories/${storyId}/media`, {
      method: 'POST',
      body: formData
    });
    if (!res.ok) return null;
    const data = await res.json();
    return data.url;
  },

  getPlaythroughs: async (storyId: string): Promise<Playthrough[]> => {
    const res = await fetchAdmin(`/api/admin/stories/${storyId}/playthroughs`);
    if (res.status === 401) throw new Error('Unauthorized');
    if (!res.ok) return [];
    return await res.json();
  },

  deletePlaythrough: async (storyId: string, playthroughId: string): Promise<void> => {
    const res = await fetchAdmin(`/api/admin/stories/${storyId}/playthroughs/${playthroughId}`, {
      method: 'DELETE'
    });
    if (!res.ok && res.status !== 204) {
      throw new Error('Failed to delete playthrough');
    }
  }
};
