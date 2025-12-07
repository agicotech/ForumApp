class Auth {
    static getUser() {
        const userStr = localStorage.getItem('user');
        return userStr ? JSON.parse(userStr) : null;
    }

    static getToken() {
        return localStorage.getItem('token');
    }

    static setAuth(token, user) {
        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify(user));
    }

    static clearAuth() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
    }

    static isAuthenticated() {
        return !!this.getToken();
    }

    static isAdmin() {
        const user = this.getUser();
        return user && user.role === 'Admin';
    }

    static isUser() {
        const user = this.getUser();
        return user && (user.role === 'User' || user.role === 'Admin');
    }

    static getUserId() {
        const user = this.getUser();
        return user ? user.id : null;
    }

    static getUsername() {
        const user = this.getUser();
        return user ? user.username : null;
    }
}
