// State
let currentTopicId = null;
let currentView = 'topics'; // 'topics', 'topic-details', 'audit'

// DOM Elements
const loginBtn = document.getElementById('loginBtn');
const registerBtn = document.getElementById('registerBtn');
const logoutBtn = document.getElementById('logoutBtn');
const changePasswordBtn = document.getElementById('changePasswordBtn');
const userInfo = document.getElementById('userInfo');
const username = document.getElementById('username');
const adminPanel = document.getElementById('adminPanel');
const createTopicBtn = document.getElementById('createTopicBtn');
const viewAuditBtn = document.getElementById('viewAuditBtn');
const searchBtn = document.getElementById('searchBtn');
const searchInput = document.getElementById('searchInput');
const topicsList = document.getElementById('topicsList');
const topicDetails = document.getElementById('topicDetails');
const auditLog = document.getElementById('auditLog');

// Modals
const loginModal = document.getElementById('loginModal');
const registerModal = document.getElementById('registerModal');
const changePasswordModal = document.getElementById('changePasswordModal');
const createTopicModal = document.getElementById('createTopicModal');

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    updateUI();
    loadTopics();
    setupEventListeners();
});

function setupEventListeners() {
    // Auth buttons
    loginBtn.addEventListener('click', () => openModal(loginModal));
    registerBtn.addEventListener('click', () => openModal(registerModal));
    logoutBtn.addEventListener('click', logout);
    changePasswordBtn.addEventListener('click', () => openModal(changePasswordModal));

    // Admin buttons
    createTopicBtn.addEventListener('click', () => openModal(createTopicModal));
    viewAuditBtn.addEventListener('click', showAuditLog);

    // Search
    searchBtn.addEventListener('click', () => loadTopics(searchInput.value));
    searchInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') loadTopics(searchInput.value);
    });

    // Navigation
    document.getElementById('backToTopicsBtn').addEventListener('click', () => {
        currentView = 'topics';
        showTopicsList();
    });
    document.getElementById('backFromAuditBtn').addEventListener('click', () => {
        currentView = 'topics';
        showTopicsList();
    });

    // Forms
    document.getElementById('loginForm').addEventListener('submit', handleLogin);
    document.getElementById('registerForm').addEventListener('submit', handleRegister);
    document.getElementById('changePasswordForm').addEventListener('submit', handleChangePassword);
    document.getElementById('createTopicForm').addEventListener('submit', handleCreateTopic);
    document.getElementById('submitMessageBtn').addEventListener('click', handleCreateMessage);

    // Modal close buttons
    document.querySelectorAll('.close').forEach(btn => {
        btn.addEventListener('click', () => {
            closeModal(btn.closest('.modal'));
        });
    });

    // Close modal on outside click
    window.addEventListener('click', (e) => {
        if (e.target.classList.contains('modal')) {
            closeModal(e.target);
        }
    });
}

function updateUI() {
    if (Auth.isAuthenticated()) {
        loginBtn.style.display = 'none';
        registerBtn.style.display = 'none';
        userInfo.style.display = 'flex';
        username.textContent = Auth.getUsername();

        if (Auth.isAdmin()) {
            adminPanel.style.display = 'flex';
        }

        if (Auth.isUser()) {
            document.getElementById('addMessageForm').style.display = 'block';
        }
    } else {
        loginBtn.style.display = 'inline-block';
        registerBtn.style.display = 'inline-block';
        userInfo.style.display = 'none';
        adminPanel.style.display = 'none';
        document.getElementById('addMessageForm').style.display = 'none';
    }
}

// Modal functions
function openModal(modal) {
    modal.style.display = 'block';
}

function closeModal(modal) {
    modal.style.display = 'none';
    modal.querySelector('form')?.reset();
}

// Auth handlers
async function handleLogin(e) {
    e.preventDefault();
    const username = document.getElementById('loginUsername').value;
    const password = document.getElementById('loginPassword').value;

    try {
        const data = await API.login(username, password);
        Auth.setAuth(data.token, data.user);
        closeModal(loginModal);
        updateUI();
        loadTopics();
        showMessage('Вход выполнен успешно!', 'success');
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

async function handleRegister(e) {
    e.preventDefault();
    const username = document.getElementById('regUsername').value;
    const email = document.getElementById('regEmail').value;
    const password = document.getElementById('regPassword').value;

    try {
        await API.register(username, email, password);
        closeModal(registerModal);
        showMessage('Регистрация успешна! Теперь вы можете войти.', 'success');
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

async function handleChangePassword(e) {
    e.preventDefault();
    const oldPassword = document.getElementById('oldPassword').value;
    const newPassword = document.getElementById('newPassword').value;

    try {
        await API.changePassword(oldPassword, newPassword);
        closeModal(changePasswordModal);
        showMessage('Пароль успешно изменен!', 'success');
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

function logout() {
    Auth.clearAuth();
    updateUI();
    currentView = 'topics';
    showTopicsList();
    loadTopics();
    showMessage('Вы вышли из системы', 'success');
}

// Topics
async function loadTopics(search = '') {
    try {
        const topics = await API.getTopics(search);
        displayTopics(topics);
    } catch (error) {
        showMessage('Ошибка загрузки тем: ' + error.message, 'error');
    }
}

function displayTopics(topics) {
    topicsList.innerHTML = '';
    if (topics.length === 0) {
        topicsList.innerHTML = '<p>Темы не найдены</p>';
        return;
    }

    topics.forEach(topic => {
        const card = document.createElement('div');
        card.className = 'topic-card';
        card.innerHTML = `
            <h3>${escapeHtml(topic.title)}</h3>
            <p>${escapeHtml(topic.description || '')}</p>
            <div class="topic-meta">
                Автор: ${escapeHtml(topic.author.username)} | 
                Сообщений: ${topic.messageCount} | 
                Создано: ${new Date(topic.createdAt).toLocaleString('ru-RU')}
            </div>
        `;
        card.addEventListener('click', () => showTopicDetails(topic.id));
        topicsList.appendChild(card);
    });
}

async function handleCreateTopic(e) {
    e.preventDefault();
    const title = document.getElementById('topicTitle').value;
    const description = document.getElementById('topicDescription').value;

    try {
        await API.createTopic(title, description);
        closeModal(createTopicModal);
        loadTopics();
        showMessage('Тема успешно создана!', 'success');
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

// Topic Details
async function showTopicDetails(topicId) {
    currentTopicId = topicId;
    currentView = 'topic-details';

    try {
        const topic = await API.getTopic(topicId);
        const messages = await API.getMessages(topicId);

        document.getElementById('topicInfo').innerHTML = `
            <h2>${escapeHtml(topic.title)}</h2>
            <p>${escapeHtml(topic.description || '')}</p>
            <div class="topic-meta">
                Автор: ${escapeHtml(topic.author.username)} | 
                Создано: ${new Date(topic.createdAt).toLocaleString('ru-RU')}
            </div>
        `;

        displayMessages(messages);

        topicsList.style.display = 'none';
        adminPanel.style.display = 'none';
        document.querySelector('.search-box').style.display = 'none';
        topicDetails.style.display = 'block';

        if (Auth.isUser()) {
            document.getElementById('addMessageForm').style.display = 'block';
        }
    } catch (error) {
        showMessage('Ошибка загрузки темы: ' + error.message, 'error');
    }
}

function displayMessages(messages) {
    const messagesList = document.getElementById('messagesList');
    messagesList.innerHTML = '';

    if (messages.length === 0) {
        messagesList.innerHTML = '<p>Сообщений пока нет. Будьте первым!</p>';
        return;
    }

    messages.forEach(message => {
        const card = document.createElement('div');
        card.className = 'message-card';
        
        const isAuthor = Auth.getUserId() === message.author.id;
        const canDelete = isAuthor || Auth.isAdmin();

        card.innerHTML = `
            <div class="message-header">
                <span class="message-author">${escapeHtml(message.author.username)}</span>
                <span class="message-date">${new Date(message.createdAt).toLocaleString('ru-RU')}</span>
            </div>
            <div class="message-text">${escapeHtml(message.text)}</div>
            ${message.updatedAt ? `<small>Изменено: ${new Date(message.updatedAt).toLocaleString('ru-RU')}</small>` : ''}
            ${canDelete ? `
                <div class="message-actions">
                    ${isAuthor ? `<button class="btn" onclick="editMessage(${message.id}, '${escapeHtml(message.text).replace(/'/g, "\\'")}')">Редактировать</button>` : ''}
                    <button class="btn btn-danger" onclick="deleteMessage(${message.id})">Удалить</button>
                </div>
            ` : ''}
        `;
        messagesList.appendChild(card);
    });
}

async function handleCreateMessage() {
    const text = document.getElementById('messageText').value.trim();
    if (!text) {
        showMessage('Введите текст сообщения', 'error');
        return;
    }

    try {
        await API.createMessage(currentTopicId, text);
        document.getElementById('messageText').value = '';
        const messages = await API.getMessages(currentTopicId);
        displayMessages(messages);
        showMessage('Сообщение добавлено!', 'success');
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

async function editMessage(id, currentText) {
    const newText = prompt('Редактировать сообщение:', currentText);
    if (newText && newText !== currentText) {
        try {
            await API.updateMessage(id, newText);
            const messages = await API.getMessages(currentTopicId);
            displayMessages(messages);
            showMessage('Сообщение обновлено!', 'success');
        } catch (error) {
            showMessage(error.message, 'error');
        }
    }
}

async function deleteMessage(id) {
    if (!confirm('Удалить это сообщение?')) return;

    try {
        await API.deleteMessage(id);
        const messages = await API.getMessages(currentTopicId);
        displayMessages(messages);
        showMessage('Сообщение удалено!', 'success');
    } catch (error) {
        showMessage(error.message, 'error');
    }
}

function showTopicsList() {
    topicDetails.style.display = 'none';
    auditLog.style.display = 'none';
    topicsList.style.display = 'block';
    document.querySelector('.search-box').style.display = 'flex';
    if (Auth.isAdmin()) {
        adminPanel.style.display = 'flex';
    }
    loadTopics();
}

// Audit Log
async function showAuditLog() {
    currentView = 'audit';

    try {
        const logs = await API.getAuditLogs();
        displayAuditLogs(logs);

        topicsList.style.display = 'none';
        topicDetails.style.display = 'none';
        adminPanel.style.display = 'none';
        document.querySelector('.search-box').style.display = 'none';
        auditLog.style.display = 'block';
    } catch (error) {
        showMessage('Ошибка загрузки журнала: ' + error.message, 'error');
    }
}

function displayAuditLogs(logs) {
    const auditList = document.getElementById('auditList');
    auditList.innerHTML = '';

    if (logs.length === 0) {
        auditList.innerHTML = '<p>Журнал пуст</p>';
        return;
    }

    logs.forEach(log => {
        const entry = document.createElement('div');
        entry.className = 'audit-entry';
        entry.innerHTML = `
            <strong>${escapeHtml(log.username)}</strong> - ${escapeHtml(log.action)}
            ${log.entityType ? `<br>Тип: ${escapeHtml(log.entityType)}` : ''}
            ${log.entityId ? ` ID: ${log.entityId}` : ''}
            ${log.details ? `<br>Детали: ${escapeHtml(log.details)}` : ''}
            <br><small>${new Date(log.timestamp).toLocaleString('ru-RU')}</small>
        `;
        auditList.appendChild(entry);
    });
}

// Utility functions
function showMessage(message, type) {
    const messageDiv = document.createElement('div');
    messageDiv.className = type === 'error' ? 'error-message' : 'success-message';
    messageDiv.textContent = message;
    
    document.querySelector('.container').insertBefore(messageDiv, document.querySelector('main'));
    
    setTimeout(() => {
        messageDiv.remove();
    }, 5000);
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}
