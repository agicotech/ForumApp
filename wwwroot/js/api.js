const API_BASE_URL = '/api';

class API {
    static async request(endpoint, options = {}) {
        const token = localStorage.getItem('token');
        const headers = {
            'Content-Type': 'application/json',
            ...options.headers
        };

        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        try {
            const response = await fetch(`${API_BASE_URL}${endpoint}`, {
                ...options,
                headers
            });

            if (!response.ok) {
                const error = await response.json().catch(() => ({ message: 'Ошибка сервера' }));
                throw new Error(error.message || `HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    }

    // Auth endpoints
    static async register(username, email, password) {
        return this.request('/auth/register', {
            method: 'POST',
            body: JSON.stringify({ username, email, password })
        });
    }

    static async login(username, password) {
        return this.request('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password })
        });
    }

    static async changePassword(oldPassword, newPassword) {
        return this.request('/auth/change-password', {
            method: 'POST',
            body: JSON.stringify({ oldPassword, newPassword })
        });
    }

    // Topics endpoints
    static async getTopics(search = '') {
        const query = search ? `?search=${encodeURIComponent(search)}` : '';
        return this.request(`/topics${query}`);
    }

    static async getTopic(id) {
        return this.request(`/topics/${id}`);
    }

    static async createTopic(title, description) {
        return this.request('/topics', {
            method: 'POST',
            body: JSON.stringify({ title, description })
        });
    }

    static async deleteTopic(id) {
        return this.request(`/topics/${id}`, {
            method: 'DELETE'
        });
    }

    // Messages endpoints
    static async getMessages(topicId, search = '') {
        const query = search ? `?search=${encodeURIComponent(search)}` : '';
        return this.request(`/messages/topic/${topicId}${query}`);
    }

    static async createMessage(topicId, text) {
        return this.request('/messages', {
            method: 'POST',
            body: JSON.stringify({ topicId, text })
        });
    }

    static async updateMessage(id, text) {
        return this.request(`/messages/${id}`, {
            method: 'PUT',
            body: JSON.stringify({ text })
        });
    }

    static async deleteMessage(id) {
        return this.request(`/messages/${id}`, {
            method: 'DELETE'
        });
    }

    // Audit endpoints
    static async getAuditLogs() {
        return this.request('/audit');
    }

    // User management endpoints
    static async getAllUsers() {
        return this.request('/auth/users');
    }

    static async promoteToAdmin(userId) {
        return this.request(`/auth/promote-to-admin/${userId}`, {
            method: 'POST'
        });
    }
}
