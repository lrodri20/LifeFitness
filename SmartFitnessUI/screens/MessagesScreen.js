// src/screens/MessagesScreen.js

import React, { useEffect, useState, useContext, useCallback } from 'react';
import {
    View,
    Text,
    FlatList,
    Image,
    TouchableOpacity,
    StyleSheet,
    SafeAreaView,
    ActivityIndicator,
    RefreshControl,
} from 'react-native';
import { useFocusEffect } from '@react-navigation/native';
import { AuthContext } from '../context/AuthContext';
import exampleImage from '../images/example1.avif';

const API_URL = 'http://localhost:5199';

export default function MessagesScreen({ navigation }) {
    const { userToken } = useContext(AuthContext);
    const [matches, setMatches] = useState([]);
    const [chats, setChats] = useState([]);
    const [loading, setLoading] = useState(true);
    const [refreshing, setRefreshing] = useState(false);

    const loadData = useCallback(async () => {
        try {
            // 1) load all matches
            const mRes = await fetch(`${API_URL}/api/matches`, {
                headers: { Authorization: `Bearer ${userToken}` },
            });
            const matchList = await mRes.json();
            setMatches(matchList);

            // 2) load only chats (matches with messages)
            const cRes = await fetch(`${API_URL}/api/chats`, {
                headers: { Authorization: `Bearer ${userToken}` },
            });
            const chatList = await cRes.json();
            setChats(chatList);
        } catch (err) {
            console.warn('Error loading messages:', err);
        }
    }, [userToken]);

    // initial load
    useEffect(() => {
        setLoading(true);
        loadData().finally(() => setLoading(false));
    }, [loadData]);

    // reload on return/focus
    useFocusEffect(
        useCallback(() => {
            loadData();
        }, [loadData])
    );

    // pull-to-refresh
    const onRefresh = useCallback(() => {
        setRefreshing(true);
        loadData().finally(() => setRefreshing(false));
    }, [loadData]);

    const renderMatch = ({ item }) => (
        <TouchableOpacity
            style={styles.matchCard}
            onPress={() => navigation.navigate('ChatScreen', { matchId: item.matchId })}
        >
            <Image
                source={
                    item.otherUser.avatarUrl
                        ? { uri: item.otherUser.avatarUrl }
                        : exampleImage
                }
                style={styles.matchAvatar}
            />
            <Text style={styles.matchName} numberOfLines={1}>
                {item.otherUser.name}
            </Text>
        </TouchableOpacity>
    );

    const renderChat = ({ item }) => (
        <TouchableOpacity
            style={styles.chatCard}
            onPress={() => navigation.navigate('ChatScreen', { matchId: item.matchId })}
        >
            <Image
                source={
                    item.otherUser.avatarUrl
                        ? { uri: item.otherUser.avatarUrl }
                        : exampleImage
                }
                style={styles.chatAvatar}
            />
            <View style={styles.chatInfo}>
                <Text style={styles.chatName}>{item.otherUser.name}</Text>
                <Text style={styles.chatMessage} numberOfLines={1}>
                    {item.lastMessage}
                </Text>
            </View>
            <Text style={styles.chatTime}>{item.time}</Text>
            {item.unreadCount > 0 && (
                <View style={styles.unreadBadge}>
                    <Text style={styles.unreadText}>{item.unreadCount}</Text>
                </View>
            )}
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
                keyExtractor={(i) => i.chatId}
                renderItem={renderChat}
                contentContainerStyle={{ paddingVertical: 8 }}
                // header = matches carousel
                ListHeaderComponent={() => (
                    <View style={styles.matchesContainer}>
                        <FlatList
                            data={matches}
                            horizontal
                            keyExtractor={(i) => i.matchId}
                            renderItem={renderMatch}
                            showsHorizontalScrollIndicator={false}
                            contentContainerStyle={styles.matchesList}
                        />
                    </View>
                )}
                refreshControl={
                    <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
                }
                ListEmptyComponent={() => (
                    <View style={styles.emptyContainer}>
                        <Text style={styles.emptyText}>No messages yet.</Text>
                    </View>
                )}
            />
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: '#fff' },
    center: { flex: 1, alignItems: 'center', justifyContent: 'center' },

    matchesContainer: {
        height: 100,
        borderBottomWidth: 1,
        borderBottomColor: '#eee',
    },
    matchesList: {
        alignItems: 'center',
        paddingHorizontal: 16,
    },
    matchCard: {
        width: 60,
        marginHorizontal: 8,
        alignItems: 'center',
    },
    matchAvatar: {
        width: 60,
        height: 60,
        borderRadius: 30,
        backgroundColor: '#ddd',
    },
    matchName: { marginTop: 4, fontSize: 12, color: '#333' },

    chatCard: {
        flexDirection: 'row',
        alignItems: 'center',
        padding: 12,
        borderBottomWidth: 1,
        borderBottomColor: '#f2f2f2',
    },
    chatAvatar: { width: 48, height: 48, borderRadius: 24, marginRight: 12 },
    chatInfo: { flex: 1 },
    chatName: { fontSize: 16, fontWeight: '600' },
    chatMessage: { fontSize: 14, color: '#666', marginTop: 2 },
    chatTime: { fontSize: 12, color: '#999', marginLeft: 8 },
    unreadBadge: {
        backgroundColor: '#4CAF50',
        borderRadius: 12,
        paddingHorizontal: 6,
        paddingVertical: 2,
        marginLeft: 8,
    },
    unreadText: { color: '#fff', fontSize: 12 },

    emptyContainer: { flex: 1, alignItems: 'center', marginTop: 50 },
    emptyText: { color: '#666', fontSize: 16 },
});
