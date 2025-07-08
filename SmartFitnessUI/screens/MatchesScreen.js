// src/screens/MatchesScreen.js

/**
 * MatchesScreen
 *
 * Displays a Tinder-style swipe deck of profile cards fetched from Unsplash.
 * Users can swipe right to "like" or left to "pass".
 */
import React, { useState, useEffect, useContext, useCallback } from 'react';
import PropTypes from 'prop-types';
import { AuthContext } from '../context/AuthContext';
import exampleImage from '../images/example1.avif'; // Placeholder image for card
// Monkey-patch React.PropTypes for deck-swiper compatibility
if (!React.PropTypes) React.PropTypes = PropTypes;

// Dynamically require Swiper so PropTypes is patched first
const SwiperModule = require('react-native-deck-swiper');
const Swiper = SwiperModule.default;

// Define expected prop types to avoid missing PropTypes errors
Swiper.propTypes = {
    cards: PropTypes.array,
    renderCard: PropTypes.func,
    onSwipedRight: PropTypes.func,
    onSwipedAll: PropTypes.func,
    cardIndex: PropTypes.number,
    backgroundColor: PropTypes.string,
    stackSize: PropTypes.number,
    stackSeparation: PropTypes.number,
    animateCardOpacity: PropTypes.bool,
    verticalSwipe: PropTypes.bool,
};

import {
    View,
    Text,
    Image,
    StyleSheet,
    Dimensions,
    SafeAreaView,
    ActivityIndicator,
    TouchableOpacity,
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import FilterModal from '../components/FilterModal';
import SortModal from '../components/SortModal';

// Get device width for responsive card sizing
const { width } = Dimensions.get('window');

// -----------------------------------------------------------------------------
// Unsplash API configuration
// Replace 'YOUR_UNSPLASH_ACCESS_KEY' with your real key from https://unsplash.com/developers
const UNSPLASH_ACCESS_KEY = 'Aslkw0ARwU8C-fquxYYCm8Ejc7X0X8LGLyeUn6o8plE';
const UNSPLASH_URL =
    `https://api.unsplash.com/photos/random?count=4&query=portrait&client_id=${UNSPLASH_ACCESS_KEY}`;
// -----------------------------------------------------------------------------
const API_URL = 'http://localhost:5199';
export default function MatchesScreen() {
    const { userToken } = useContext(AuthContext);
    // State: array of profile objects, and loading flag
    const [cards, setCards] = useState([]);
    const [loading, setLoading] = useState(true);
    const [filterVisible, setFilterVisible] = useState(false);
    const [sortVisible, setSortVisible] = useState(false);
    const [sortBy, setSortBy] = useState('Recent');
    const [filters, setFilters] = useState({ minAge: null, maxAge: null, gender: null });
    // On mount, fetch random portrait images from Unsplash
    const noCards = cards.length === 0;
    const loadData = useCallback(async () => {
        setLoading(true);
        try {
            // build query params
            const params = new URLSearchParams();
            params.append('sortBy', sortBy.toLowerCase());
            if (filters.minAge !== null) params.append('minAge', filters.minAge);
            if (filters.maxAge !== null) params.append('maxAge', filters.maxAge);
            if (filters.gender) params.append('gender', filters.gender);
            const url = `${API_URL}/api/matches?${params.toString()}`;

            const resp = await fetch(url, { headers: { Authorization: `Bearer ${userToken}` } });
            const result = await resp.json();
            const profiles = result.map(m => ({
                id: m.partner.userId,
                name: m.partner.displayName,
                bio: m.partner.bio || 'No bio',
                city: m.partner.city,
                age: m.partner.age,
                state: m.partner.state,
                distance: m.partner.distanceMiles,
            }));
            setCards(profiles);
        } catch (err) {
            console.warn('Matches load error:', err);
        } finally {
            setLoading(false);
        }
    }, [userToken, sortBy, filters]);
    useEffect(() => {
        loadData();
    }, [loadData]);
    /**
     * getRandomAge
     * Returns a random integer age between 22 and 45.
     */
    const getRandomAge = () => Math.floor(Math.random() * 24) + 22;

    /**
     * onSwipedRight
     * Callback when a card is swiped right ("like").
     */
    const onSwipedRight = index => {
        if (noCards || !cards[index]) return;
        const match = cards[index];
        fetch(`${API_URL}/api/match-requests/${match.id}`, {
            method: 'POST',
            headers: { Authorization: `Bearer ${userToken}` }
        }).catch(console.error);
    };

    /**
     * onSwipedAll
     * Callback when all cards have been swiped.
     */
    const onSwipedAll = () => console.log('No more profiles to swipe');

    /**
     * renderCard
     * Renders each card component from the `cards` array.
     */
    const renderCard = card => {
        // guard against undefined or empty cards
        if (card) {
            return (
                <View style={styles.card} key={card.id}>
                    <Image source={exampleImage} style={styles.cardImage} />
                    <View style={styles.infoOverlay}>
                        <Text style={styles.nameText}>{card.name}, <Text style={styles.ageText}>{card.age}</Text></Text>
                        <Text style={styles.locationText}>{card.city}, {card.state}</Text>
                    </View>
                </View>
            );
        }
    }
    const handleApplyFilters = (newFilters) => {
        setFilters(newFilters);
        setFilterVisible(false);
        // Immediately reload data with new filters
        loadData();
    };
    // Show loading spinner while fetching images
    if (loading) {
        return (
            <SafeAreaView style={styles.loaderContainer}>
                <ActivityIndicator size="large" color="#4CAF50" />
            </SafeAreaView>
        );
    }
    // Only show swipe labels when there are cards
    const overlayLabelsConfig = noCards
        ? {}
        : {
            left: {
                title: 'NOPE',
                style: {
                    label: { backgroundColor: 'red', color: 'white', fontSize: 24 },
                    wrapper: {
                        flexDirection: 'column',
                        alignItems: 'flex-end',
                        justifyContent: 'flex-start',
                        marginTop: 20,
                        marginLeft: -20,
                    },
                },
            },
            right: {
                title: 'LIKE',
                style: {
                    label: { backgroundColor: '#4CAF50', color: 'white', fontSize: 24 },
                    wrapper: {
                        flexDirection: 'column',
                        alignItems: 'flex-start',
                        justifyContent: 'flex-start',
                        marginTop: 20,
                        marginLeft: 20,
                    },
                },
            },
        };

    // Main render: the Swiper deck inside a SafeAreaView
    return (
        <SafeAreaView style={styles.container}>
            <View style={styles.searchRow}>
                <TouchableOpacity onPress={() => setFilterVisible(true)} style={styles.filterBtn}>
                    <Ionicons name="filter-outline" size={24} color="#555" />
                </TouchableOpacity>
                <TouchableOpacity onPress={() => setSortVisible(true)} style={styles.sortBtn}>
                    <Text style={styles.sortText}>{sortBy} <Ionicons name="chevron-down-outline" size={16} /></Text>
                </TouchableOpacity>
            </View>
            {noCards ? (
                <View style={styles.noMatchesContainer}>
                    <Text style={styles.noMatchesText}>No more matches available</Text>
                </View>
            ) : (
                <Swiper
                    cards={cards}
                    renderCard={renderCard}
                    onSwipedRight={onSwipedRight}
                    onSwipedAll={onSwipedAll}
                    cardIndex={0}
                    backgroundColor="transparent"
                    stackSize={3}
                    stackSeparation={15}
                    animateCardOpacity
                    verticalSwipe={!noCards}
                    disableRightSwipe={noCards}
                    disableLeftSwipe={noCards}
                    overlayLabels={overlayLabelsConfig}
                    containerStyle={styles.swiper}
                />
            )}
            <FilterModal
                visible={filterVisible}
                onClose={() => setFilterVisible(false)}
                onApply={handleApplyFilters}
            />
            <SortModal
                visible={sortVisible}
                initial={sortBy}
                onClose={() => setSortVisible(false)}
                onSelect={value => setSortBy(value)}
            />

        </SafeAreaView>
    );
}

// -----------------------------------------------------------------------------
// Styles for MatchesScreen
// -----------------------------------------------------------------------------
const styles = StyleSheet.create({
    container: {
        flex: 1,
        backgroundColor: '#f9f9f9',
        alignItems: 'center',
        justifyContent: 'center',
    },
    loaderContainer: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: '#f9f9f9',
    },
    swiper: {
        flex: 1,
        paddingTop: 20,
    },
    card: {
        width: width * 0.9,
        height: width * 1.1,
        borderRadius: 16,
        backgroundColor: '#fff',
        overflow: 'hidden',
        elevation: 3,
    },
    cardImage: {
        width: '100%',
        height: '100%',
    },
    infoOverlay: {
        position: 'absolute',
        bottom: 0,
        left: 0,
        right: 0,
        padding: 16,
        backgroundColor: 'rgba(0,0,0,0.4)',
    },
    nameText: {
        fontSize: 24,
        fontWeight: 'bold',
        color: '#fff',
    },
    ageText: {
        fontWeight: '600',
        color: '#fff',
    },
    locationText: {
        fontSize: 16,
        color: '#fff',
        marginTop: 4,
    },
    container: { flex: 1, backgroundColor: '#f9f9f9', paddingTop: 32 },
    searchRow: { flexDirection: 'row', alignItems: 'center', margin: 16 },
    searchInput: { flex: 1, height: 40, borderColor: '#ccc', borderWidth: 1, borderRadius: 8, paddingHorizontal: 12 },
    filterBtn: { marginLeft: 8 },
    sortBtn: {
        marginLeft: 8,
        paddingVertical: 8,
        paddingHorizontal: 12,
        backgroundColor: '#fff',
        borderRadius: 8,
        borderWidth: 1,
        borderColor: '#ccc',
        flexDirection: 'row',
        alignItems: 'center',
    },
    sortText: {
        fontSize: 14,
        color: '#333',
        marginRight: 4,
    },
    noMatchesContainer: {
        flex: 1,
        justifyContent: 'center',
        alignItems: 'center',
        paddingTop: 50,
    },
    noMatchesText: {
        fontSize: 18,
        color: '#999',
    },
});
