// src/screens/MessagesScreen.js
import React, { useEffect, useState, useContext } from 'react';
import {
    View,
    Text,
    FlatList,
    Image,
    TouchableOpacity,
    StyleSheet,
    SafeAreaView,
    ActivityIndicator
} from 'react-native';
import { AuthContext } from '../context/AuthContext';

// Dummy chat data; replace with real API response
const DUMMY_CHATS = [
    {
        id: 'c1',
        name: 'Alice',
        image: 'https://placekitten.com/200/200',
        lastMessage: 'Sounds good, see you then!',
        time: '2:45 PM',
        unread: 2,
    },
    {
        id: 'c2',
        name: 'Jamal',
        image: 'https://placekitten.com/201/200',
        lastMessage: 'Can we reschedule?',
        time: '1:15 PM',
        unread: 0,
    },
    {
        id: 'c3',
        name: 'Taylor',
        image: 'https://placekitten.com/202/200',
        lastMessage: 'Great game last night!',
        time: 'Yesterday',
        unread: 1,
    },
];

export default function MessagesScreen({ navigation }) {
    const { userToken } = useContext(AuthContext);
    const [chats, setChats] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        // TODO: Fetch real chats from backend using userToken
        setTimeout(() => {
            setChats(DUMMY_CHATS);
            setLoading(false);
        }, 1000);
    }, []);

    const renderItem = ({ item }) => (
        <TouchableOpacity
            style={styles.chatCard}
            onPress={() => navigation.navigate('Chat', { chatId: item.id })}
        >
            <Image source={{ uri: item.image }} style={styles.avatar} />
            <View style={styles.chatInfo}>
                <View style={styles.chatHeader}>
                    <Text style={styles.name}>{item.name}</Text>
                    <Text style={styles.time}>{item.time}</Text>
                </View>
                <View style={styles.messageRow}>
                    <Text style={styles.lastMessage} numberOfLines={1}>
                        {item.lastMessage}
                    </Text>
                    {item.unread > 0 && (
                        <View style={styles.unreadBadge}>
                            <Text style={styles.unreadText}>{item.unread}</Text>
                        </View>
                    )}
                </View>
            </View>
        </TouchableOpacity>
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
                data={chats}
                keyExtractor={(item) => item.id}
                renderItem={renderItem}
                contentContainerStyle={chats.length === 0 && styles.center}
                ListEmptyComponent={<Text style={styles.emptyText}>No messages yet.</Text>}
            />
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: '#fff',
    },
    center: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
    },
    chatCard: {
        flexDirection: 'row',
        padding: 12,
        borderBottomWidth: 1,
        borderColor: '#eee',
        alignItems: 'center',
    },
    avatar: {
        width: 50,
        height: 50,
        borderRadius: 25,
        marginRight: 12,
    },
    chatInfo: {
        flex: 1,
    },
    chatHeader: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginBottom: 4,
    },
    name: {
        fontSize: 16,
        fontWeight: '600',
        color: '#333',
    },
    time: {
        fontSize: 12,
        color: '#999',
    },
    messageRow: {
        flexDirection: 'row',
        alignItems: 'center',
    },
    lastMessage: {
        flex: 1,
        fontSize: 14,
        color: '#555',
    },
    unreadBadge: {
        backgroundColor: '#4CAF50',
        borderRadius: 12,
        paddingHorizontal: 6,
        marginLeft: 8,
    },
    unreadText: {
        color: '#fff',
        fontSize: 12,
    },
    emptyText: {
        color: '#777',
        fontSize: 16,
    },
});
