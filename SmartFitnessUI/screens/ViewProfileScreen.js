// src/screens/ViewProfileScreen.js
import React, { useEffect, useState, useContext } from 'react';
import {
    View,
    Text,
    Image,
    StyleSheet,
    SafeAreaView,
    ActivityIndicator,
    ScrollView,
    TouchableOpacity,
    Alert
} from 'react-native';
import { AuthContext } from '../context/AuthContext';

const API_URL = 'http://localhost:5199';

export default function ViewProfileScreen({ route, navigation }) {
    const { userToken } = useContext(AuthContext);
    const { fromUserId } = route.params; // passed when navigating from LikesScreen

    const [profile, setProfile] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadProfile = async () => {
            try {
                const resp = await fetch(`${API_URL}/api/Profile/${fromUserId}`, {
                    headers: { Authorization: `Bearer ${userToken}` }
                });
                const data = await resp.json();
                if (!resp.ok) throw new Error(data.message || 'Failed to load profile');
                setProfile(data);
            } catch (err) {
                console.warn(err);
                Alert.alert('Error', 'Could not load profile');
                navigation.goBack();
            } finally {
                setLoading(false);
            }
        };
        loadProfile();
    }, [fromUserId, userToken]);

    const handleLikeBack = async () => {
        try {
            const resp = await fetch(`${API_URL}/api/matches/${fromUserId}/like`, {
                method: 'POST',
                headers: { Authorization: `Bearer ${userToken}` }
            });
            if (!resp.ok) throw new Error('Failed to like back');
            Alert.alert('Liked', 'You have liked them back!');
            navigation.goBack();
        } catch (err) {
            console.warn(err);
            Alert.alert('Error', 'Could not send like');
        }
    };

    const handleReject = async () => {
        try {
            // Call reject endpoint
            const resp = await fetch(`${API_URL}/api/likes/${fromUserId}/reject`, {
                method: 'POST',
                headers: { Authorization: `Bearer ${userToken}` }
            });
            if (!resp.ok) throw new Error('Failed to reject');
            Alert.alert('Rejected', 'You have dismissed this like');
            navigation.goBack();
        } catch (err) {
            console.warn(err);
            Alert.alert('Error', 'Could not reject');
        }
    };

    if (loading) {
        return (
            <SafeAreaView style={styles.center}>
                <ActivityIndicator size="large" color="#4CAF50" />
            </SafeAreaView>
        );
    }

    if (!profile) return null;

    return (
        <SafeAreaView style={styles.container}>
            <ScrollView contentContainerStyle={styles.content}>
                <Image source={{ uri: profile.profilePictureUrl }} style={styles.avatar} />
                <Text style={styles.name}>{profile.displayName}, {profile.age}</Text>
                <Text style={styles.location}>{profile.city}, {profile.state}</Text>
                <Text style={styles.bio}>{profile.bio || 'No bio available.'}</Text>

                <View style={styles.section}>
                    <Text style={styles.sectionTitle}>Fitness Level</Text>
                    <Text>{profile.fitnessLevelName}</Text>
                </View>

                <View style={styles.section}>
                    <Text style={styles.sectionTitle}>Activities</Text>
                    {profile.activities.map((act, i) => (
                        <Text key={i}>- {act}</Text>
                    ))}
                </View>

                <View style={styles.buttonsRow}>
                    <TouchableOpacity style={[styles.button, styles.rejectBtn]} onPress={handleReject}>
                        <Text style={styles.rejectText}>Reject</Text>
                    </TouchableOpacity>
                    <TouchableOpacity style={[styles.button, styles.likeBtn]} onPress={handleLikeBack}>
                        <Text style={styles.likeText}>Like Back</Text>
                    </TouchableOpacity>
                </View>
            </ScrollView>
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: '#fff' },
    center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
    content: { alignItems: 'center', padding: 16 },
    avatar: { width: 120, height: 120, borderRadius: 60, marginVertical: 16 },
    name: { fontSize: 24, fontWeight: 'bold' },
    location: { fontSize: 16, color: '#666', marginBottom: 12 },
    bio: { textAlign: 'center', marginBottom: 20 },
    section: { width: '100%', marginBottom: 16 },
    sectionTitle: { fontSize: 16, fontWeight: '600', marginBottom: 8 },
    buttonsRow: { flexDirection: 'row', justifyContent: 'space-around', width: '100%', marginTop: 24 },
    button: { flex: 1, marginHorizontal: 8, paddingVertical: 12, borderRadius: 8, alignItems: 'center' },
    likeBtn: { backgroundColor: '#4CAF50' },
    likeText: { color: '#fff', fontSize: 16 },
    rejectBtn: { backgroundColor: '#eee' },
    rejectText: { color: '#333', fontSize: 16 },
});
