// src/screens/MatchesScreen.js

/**
 * MatchesScreen
 *
 * Displays a Tinder-style swipe deck of profile cards fetched from Unsplash.
 * Users can swipe right to "like" or left to "pass".
 */
import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';

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
    ActivityIndicator
} from 'react-native';

// Get device width for responsive card sizing
const { width } = Dimensions.get('window');

// -----------------------------------------------------------------------------
// Unsplash API configuration
// Replace 'YOUR_UNSPLASH_ACCESS_KEY' with your real key from https://unsplash.com/developers
const UNSPLASH_ACCESS_KEY = 'Aslkw0ARwU8C-fquxYYCm8Ejc7X0X8LGLyeUn6o8plE';
const UNSPLASH_URL =
    `https://api.unsplash.com/photos/random?count=4&query=portrait&client_id=${UNSPLASH_ACCESS_KEY}`;
// -----------------------------------------------------------------------------

export default function MatchesScreen() {
    // State: array of profile objects, and loading flag
    const [cards, setCards] = useState([]);
    const [loading, setLoading] = useState(true);

    // On mount, fetch random portrait images from Unsplash
    useEffect(() => {
        fetch(UNSPLASH_URL)
            .then(res => res.json())
            .then(data => {
                // Map Unsplash response into our profile format
                const profiles = data.map(photo => ({
                    id: photo.id,
                    name: `${photo.user.first_name} ${photo.user.last_name || ''}`.trim()
                        + `, ${getRandomAge()}`,
                    image: photo.urls.regular,
                    bio: photo.user.bio || 'Hello there!',
                }));
                setCards(profiles);
            })
            .catch(err => {
                console.warn('Unsplash fetch error:', err);
                // Fallback: use static placeholder if API call fails
                setCards([
                    {
                        id: 'fallback-1',
                        name: 'Fallback User, 29',
                        image: 'https://placekitten.com/400/500',
                        bio: 'Unable to load images from Unsplash.',
                    },
                ]);
            })
            .finally(() => setLoading(false));
    }, []);

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
        const matched = cards[index];
        console.log('Matched with:', matched.name);
        // TODO: send match event to backend
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
    const renderCard = card => (
        <View style={styles.card} key={card.id}>
            <Image source={{ uri: card.image }} style={styles.cardImage} />
            <View style={styles.cardDetails}>
                <Text style={styles.cardName}>{card.name}</Text>
                <Text style={styles.cardBio}>{card.bio}</Text>
            </View>
        </View>
    );

    // Show loading spinner while fetching images
    if (loading) {
        return (
            <SafeAreaView style={styles.loaderContainer}>
                <ActivityIndicator size="large" color="#4CAF50" />
            </SafeAreaView>
        );
    }

    // Main render: the Swiper deck inside a SafeAreaView
    return (
        <SafeAreaView style={styles.container}>
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
                verticalSwipe={false}
                overlayLabels={{
                    left: {
                        title: 'NOPE',
                        style: {
                            label: {
                                backgroundColor: 'red',
                                color: 'white',
                                fontSize: 24,
                            },
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
                            label: {
                                backgroundColor: '#4CAF50',
                                color: 'white',
                                fontSize: 24,
                            },
                            wrapper: {
                                flexDirection: 'column',
                                alignItems: 'flex-start',
                                justifyContent: 'flex-start',
                                marginTop: 20,
                                marginLeft: 20,
                            },
                        },
                    },
                }}
                containerStyle={styles.swiper}
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
        backgroundColor: 'white',
        shadowColor: '#000',
        shadowOffset: { width: 0, height: 2 },
        shadowOpacity: 0.1,
        shadowRadius: 5,
        elevation: 3,
        overflow: 'hidden',
    },
    cardImage: {
        width: '100%',
        height: '75%',
    },
    cardDetails: {
        padding: 16,
    },
    cardName: {
        fontSize: 22,
        fontWeight: '600',
        marginBottom: 8,
    },
    cardBio: {
        fontSize: 14,
        color: '#555',
    },
});
