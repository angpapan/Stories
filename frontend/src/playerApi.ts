export interface PlayerChoice {
  id: string;
  text: string;
  media: any[];
}

export interface PlayerNode {
  text: string;
  media: any[];
  choices: PlayerChoice[];
  isEnding: boolean;
}

export interface PlaythroughResponse {
  playthroughId: string;
  status: 'InProgress' | 'Completed';
  node: PlayerNode;
}

export interface ChoiceResponse {
  status: 'InProgress' | 'Completed';
  node: PlayerNode;
}

export interface StoryInfoResponse {
  title?: string;
  description?: string;
  requiresPassword: boolean;
  passwordHint?: string;
}

export const playerApi = {
  getStoryInfo: async (storyId: string): Promise<StoryInfoResponse | { error: string, status: number }> => {
    const res = await fetch(`/api/stories/${storyId}`);
    if (!res.ok) {
      return { error: 'Story not found', status: res.status };
    }
    return await res.json();
  },

  unlockStory: async (storyId: string, password: string): Promise<StoryInfoResponse | { error: string, status: number }> => {
    const res = await fetch(`/api/stories/${storyId}/unlock`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ password })
    });
    if (!res.ok) {
      if (res.status === 401) return { error: 'Incorrect password', status: 401 };
      return { error: 'Story not found', status: res.status };
    }
    return await res.json();
  },

  startPlaythrough: async (storyId: string, password?: string): Promise<PlaythroughResponse | { error: string, status: number }> => {
    const res = await fetch(`/api/stories/${storyId}/playthroughs`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ password })
    });
    if (!res.ok) {
      let error = 'An error occurred';
      if (res.status === 401) error = 'Unauthorized';
      if (res.status === 400) {
        const text = await res.text();
        // Extract plain text error string if possible
        try {
           const parsed = JSON.parse(text);
           error = parsed.detail || text;
        } catch {
           error = text;
        }
        if (text.includes("maximum allowed playthroughs")) {
            error = "This story has reached its maximum allowed playthroughs and is locked.";
        }
      }
      if (res.status === 404) error = 'Story not found';
      return { error, status: res.status };
    }
    return await res.json();
  },

  makeChoice: async (playthroughId: string, choiceId: string): Promise<ChoiceResponse | { error: string }> => {
    const res = await fetch(`/api/playthroughs/${playthroughId}/choices`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ choiceId })
    });
    if (!res.ok) {
      return { error: 'Failed to make choice' };
    }
    return await res.json();
  },

  resumePlaythrough: async (playthroughId: string): Promise<ChoiceResponse | { error: string, status: number }> => {
    const res = await fetch(`/api/playthroughs/${playthroughId}/current`);
    if (!res.ok) {
      return { error: 'Failed to resume playthrough', status: res.status };
    }
    return await res.json();
  }
};
