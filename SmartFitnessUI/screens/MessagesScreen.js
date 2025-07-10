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
    ActivityIndicator,
} from 'react-native';
import { AuthContext } from '../context/AuthContext';
import exampleImage from '../images/example1.avif';

const apiBaseUrl = 'http://localhost:5199';

export default function MessagesScreen({ navigation }) {
    const { userToken } = useContext(AuthContext);
    const [matches, setMatches] = useState([]);
    const [chats, setChats] = useState([]);
    const [selectedMatch, setSelectedMatch] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadData = async () => {
            try {
                const [mRes, cRes] = await Promise.all([
                    fetch(`${apiBaseUrl}/api/Matches`, {
                        headers: { Authorization: `Bearer ${userToken}` },
                    }),
                    fetch(`${apiBaseUrl}/api/Chats`, {
                        headers: { Authorization: `Bearer ${userToken}` },
                    }),
                ]);

                if (!mRes.ok || !cRes.ok) {
                    throw new Error('Failed to fetch');
                }

                const [matchList, chatList] = await Promise.all([
                    mRes.json(),
                    cRes.json(),
                ]);

                setMatches(matchList);
                setChats(chatList);
                // Optionally pre-select the first match
                if (matchList.length) {
                    setSelectedMatch(matchList[0]);
                }
            } catch (err) {
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        loadData();
    }, [userToken]);

    const handleSelectMatch = (item) => {
        setSelectedMatch(item);
        navigation.navigate('Chat', {
            chatId: item.matchId,
            matchId: item.matchId,
        });
    };

    const renderMatchItem = ({ item }) => (
        <TouchableOpacity
            style={styles.matchItem}
            onPress={() => handleSelectMatch(item)}
        >
            <Image
                source={exampleImage}
                style={styles.matchAvatar}
                defaultSource={exampleImage}
            />
            <Text style={styles.matchName} numberOfLines={1}>
                {item.otherUser.name}
            </Text>
        </TouchableOpacity>
    );

    const renderChatItem = ({ item }) => (
        <TouchableOpacity
            style={styles.chatCard}
            onPress={() =>
                navigation.navigate('Chat', {
                    chatId: item.chatId,
                    matchId: item.matchId,
                })
            }
        >
            <Image
                source={exampleImage}
                style={styles.avatar}
                defaultSource={exampleImage}
            />
            <View style={styles.chatInfo}>
                <View style={styles.chatHeader}>
                    <Text style={styles.name}>{item.otherUser.name}</Text>
                    <Text style={styles.time}>{item.time}</Text>
                </View>
                <View style={styles.messageRow}>
                    <Text style={styles.lastMessage} numberOfLines={1}>
                        {item.lastMessage}
                    </Text>
                    {item.unreadCount > 0 && (
                        <View style={styles.unreadBadge}>
                            <Text style={styles.unreadText}>{item.unreadCount}</Text>
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
            {/* Matches strip */}
            <View style={styles.matchesWrapper}>
                {matches.length > 0 ? (
                    <FlatList
                        data={matches}
                        horizontal
                        showsHorizontalScrollIndicator={false}
                        keyExtractor={(m) => m.matchId.toString()}
                        renderItem={renderMatchItem}
                        contentContainerStyle={styles.matchesList}
                    />
                ) : (
                    <Text style={styles.noMatchesText}>No matches yet.</Text>
                )}
            </View>

            {/* Chats or empty state */}
            <View style={styles.chatContainer}>
                {chats.length > 0 ? (
                    <FlatList
                        data={chats}
                        keyExtractor={(c) => c.chatId.toString()}
                        renderItem={renderChatItem}
                    />
                ) : (
                    <View style={styles.emptyState}>
                        {selectedMatch && (
                            <Image
                                source={{
                                    uri: selectedMatch.otherUser.avatarUrl ?? undefined,
                                }}
                                style={styles.emptyAvatar}
                                defaultSource={exampleImage}
                            />
                        )}
                        <Text style={styles.emptyName}>
                            {selectedMatch?.otherUser.name ?? ''}
                        </Text>
                        <Text style={styles.emptyText}>No messages yet.</Text>
                    </View>
                )}
            </View>
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

    /* Matches */
    matchesWrapper: {
        paddingVertical: 8,
        borderBottomWidth: StyleSheet.hairlineWidth,
        borderBottomColor: '#eee',
        backgroundColor: '#fff',
    },
    matchesList: {
        paddingHorizontal: 16,
        alignItems: 'center',
    },
    matchItem: {
        marginRight: 16,
        alignItems: 'center',
    },
    matchAvatar: {
        width: 56,
        height: 56,
        borderRadius: 28,
        backgroundColor: '#ddd',
    },
    matchName: {
        marginTop: 4,
        fontSize: 12,
        color: '#333',
    },
    noMatchesText: {
        padding: 16,
        fontSize: 14,
        color: '#888',
        textAlign: 'center',
    },

    /* Chats */
    chatContainer: {
        flex: 1,
        backgroundColor: '#fff',
    },
    chatCard: {
        flexDirection: 'row',
        padding: 16,
        borderBottomWidth: StyleSheet.hairlineWidth,
        borderBottomColor: '#eee',
    },
    avatar: {
        width: 48,
        height: 48,
        borderRadius: 24,
        backgroundColor: '#ddd',
    },
    chatInfo: {
        flex: 1,
        marginLeft: 12,
        justifyContent: 'center',
    },
    chatHeader: {
        flexDirection: 'row',
        justifyContent: 'space-between',
    },
    name: {
        fontSize: 16,
        fontWeight: '500',
    },
    time: {
        fontSize: 12,
        color: '#888',
    },
    messageRow: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginTop: 4,
    },
    lastMessage: {
        flex: 1,
        fontSize: 14,
        color: '#555',
    },
    unreadBadge: {
        backgroundColor: '#4CAF50',
        borderRadius: 10,
        paddingHorizontal: 6,
        paddingVertical: 2,
        marginLeft: 8,
    },
    unreadText: {
        color: '#fff',
        fontSize: 12,
    },

    /* Empty state */
    emptyState: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
        paddingHorizontal: 16,
    },
    emptyAvatar: {
        width: 80,
        height: 80,
        borderRadius: 40,
        marginBottom: 12,
    },
    emptyName: {
        fontSize: 16,
        fontWeight: '600',
        color: '#333',
        marginBottom: 4,
    },
    emptyText: {
        fontSize: 14,
        color: '#888',
    },
});
