// src/screens/ChatScreen.js

import React, { useEffect, useState, useContext, useLayoutEffect } from 'react';
import {
    View,
    Text,
    FlatList,
    TextInput,
    TouchableOpacity,
    KeyboardAvoidingView,
    Platform,
    SafeAreaView,
    StyleSheet,
    ActivityIndicator,
} from 'react-native';
import { AuthContext } from '../context/AuthContext';

const API_URL = 'http://localhost:5199';

export default function ChatScreen({ route, navigation }) {
    const { userToken } = useContext(AuthContext);
    const { matchId } = route.params;

    const [messages, setMessages] = useState([]);
    const [text, setText] = useState('');
    const [loading, setLoading] = useState(true);

    // load messages
    const loadMessages = async () => {
        try {
            const res = await fetch(
                `${API_URL}/api/chats/${matchId}/messages`,
                { headers: { Authorization: `Bearer ${userToken}` } }
            );
            const data = await res.json();
            setMessages(data);
        } catch (e) {
            console.warn(e);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadMessages();
    }, [matchId]);

    // header title = other user's name
    useLayoutEffect(() => {
        // optional: fetch other user’s profile here to set navigation title
        navigation.setOptions({ title: 'Chat' });
    }, [navigation]);

    const sendMessage = async () => {
        if (!text.trim()) return;
        try {
            await fetch(`${API_URL}/api/chats/${matchId}/messages`, {
                method: 'POST',
                headers: {
                    Authorization: `Bearer ${userToken}`,
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ Text: text.trim() }),
            });
            setText('');
            loadMessages();
        } catch (e) {
            console.warn(e);
        }
    };

    const renderItem = ({ item }) => (
        <View
            style={[
                styles.bubble,
                item.senderId === /* your userId */ item.senderId
                    ? styles.mine
                    : styles.theirs,
            ]}
        >
            <Text style={styles.bubbleText}>{item.text}</Text>
            <Text style={styles.bubbleTime}>
                {new Date(item.sentAt).toLocaleTimeString()}
            </Text>
        </View>
    );

    if (loading) {
        return (
            <View style={styles.center}>
                <ActivityIndicator size="large" color="#4CAF50" />
            </View>
        );
    }

    return (
        <SafeAreaView style={styles.container}>
            <FlatList
                data={messages}
                keyExtractor={(m) => m.id.toString()}
                renderItem={renderItem}
                contentContainerStyle={styles.list}
                inverted
            />
            <KeyboardAvoidingView
                behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
            >
                <View style={styles.inputRow}>
                    <TextInput
                        style={styles.input}
                        value={text}
                        onChangeText={setText}
                        placeholder="Type a message..."
                    />
                    <TouchableOpacity style={styles.sendBtn} onPress={sendMessage}>
                        <Text style={styles.sendText}>Send</Text>
                    </TouchableOpacity>
                </View>
            </KeyboardAvoidingView>
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: '#fff' },
    center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
    list: { padding: 16, paddingBottom: 0 },
    bubble: {
        marginVertical: 4,
        padding: 8,
        borderRadius: 8,
        maxWidth: '75%',
    },
    mine: {
        backgroundColor: '#DCF8C6',
        alignSelf: 'flex-end',
    },
    theirs: {
        backgroundColor: '#EEE',
        alignSelf: 'flex-start',
    },
    bubbleText: { fontSize: 16 },
    bubbleTime: { fontSize: 10, color: '#666', marginTop: 4, alignSelf: 'flex-end' },
    inputRow: {
        flexDirection: 'row',
        padding: 8,
        borderTopWidth: 1,
        borderColor: '#DDD',
        alignItems: 'center',
    },
    input: {
        flex: 1,
        paddingHorizontal: 12,
        paddingVertical: 8,
        backgroundColor: '#F2F2F2',
        borderRadius: 20,
        marginRight: 8,
    },
    sendBtn: {
        backgroundColor: '#4CAF50',
        paddingHorizontal: 16,
        paddingVertical: 8,
        borderRadius: 20,
    },
    sendText: { color: '#fff', fontSize: 16 },
});
