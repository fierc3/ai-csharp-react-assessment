import { useState } from 'react';
import './App.css';

interface ChatMessage {
    sender: 'user' | 'bot';
    message: string;
}

function App() {
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [input, setInput] = useState('');
    const [loading, setLoading] = useState(false);

    async function sendMessage(e: React.FormEvent) {
        e.preventDefault();
        if (!input.trim()) return;
        const userMsg: ChatMessage = { sender: 'user', message: input };
        setMessages(msgs => [...msgs, userMsg]);
        setInput('');
        setLoading(true);
        const botMsg = await fetchBotReply(input);
        setMessages(msgs => [...msgs, botMsg]);
        setLoading(false);
    }

    async function fetchBotReply(text: string): Promise<ChatMessage> {
        return { sender: 'bot', message: "Frontend static." };
    }

    return (
        <div className="chat-container">
            <h1>Simple Chat</h1>
            <div className="chat-window">
                {messages.length === 0 && <div className="chat-empty">Start the conversation!</div>}
                {messages.map((msg, idx) => (
                    <div key={idx} className={`chat-message ${msg.sender}`}>
                        <span>{msg.message}</span>
                    </div>
                ))}
                {loading && <div className="chat-message bot"><span>Bot is typing...</span></div>}
            </div>
            <form className="chat-input-row" onSubmit={sendMessage}>
                <input
                    className="chat-input"
                    type="text"
                    value={input}
                    onChange={e => setInput(e.target.value)}
                    placeholder="Type your message..."
                    disabled={loading}
                />
                <button type="submit" disabled={loading || !input.trim()}>Send</button>
            </form>
        </div>
    );
}
export default App;