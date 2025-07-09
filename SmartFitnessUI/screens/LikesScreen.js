// src/screens/LikesScreen.js
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
import exampleImage from '../images/example1.avif';

const API_URL = 'http://localhost:5199';

export default function LikesScreen({ navigation }) {
    const { userToken } = useContext(AuthContext);
    const [likes, setLikes] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadLikes = async () => {
            setLoading(true);
            try {
                const resp = await fetch(`${API_URL}/api/match-requests/incoming`, {
                    headers: { Authorization: `Bearer ${userToken}` }
                });
                const data = await resp.json();
                if (!resp.ok) throw new Error(data.message || 'Failed to load likes');

                // Map API LikeDto to UI model
                const list = data.map(like => ({
                    id: like.likeId.toString(),
                    name: like.from.displayName + ', ' + (like.from.age || ''),
                    image: like.from.profilePictureUrl || exampleImage,
                    fromUserId: like.from.userId,
                }));
                setLikes(list);
            } catch (err) {
                console.warn('Failed to fetch likes:', err);
                setLikes([]);
            } finally {
                setLoading(false);
            }
        };
        if (userToken) loadLikes();
    }, [userToken]);

    const renderItem = ({ item }) => (
        < View style={styles.card} key={item.id} >
            <Image
                source={exampleImage}
                style={styles.avatar}
            />
            <Text style={styles.name}>{item.name}</Text>
            <TouchableOpacity
                style={styles.button}
                onPress={() => navigation.navigate('ViewProfile', { fromUserId: parseInt(item.fromUserId, 10) })}
            >
                <Text style={styles.buttonText}>View</Text>
            </TouchableOpacity>
        </View >
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
            {likes.length === 0 ? (
                <View style={styles.center}>
                    <Text style={styles.emptyText}>No one has liked you yet.</Text>
                </View>
            ) : (
                <FlatList
                    data={likes}
                    keyExtractor={item => item.id}
                    renderItem={renderItem}
                    contentContainerStyle={styles.list}
                    numColumns={2}
                />
            )}
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: '#f9f9f9', },
    list: { padding: 16 },
    card: {
        flex: 1,
        alignItems: 'center',
        margin: 8,
        backgroundColor: '#f9f9f9',
        borderRadius: 12,
        padding: 16,
        elevation: 2,
    },
    avatar: {
        width: 80,
        height: 80,
        borderRadius: 40,
        marginBottom: 12,
    },
    name: {
        fontSize: 16,
        fontWeight: '600',
        marginBottom: 8,
    },
    button: {
        backgroundColor: '#4CAF50',
        paddingVertical: 6,
        paddingHorizontal: 20,
        borderRadius: 20,
    },
    buttonText: { color: '#fff', fontSize: 14 },
    center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
    emptyText: { color: '#555', fontSize: 16 },
});
