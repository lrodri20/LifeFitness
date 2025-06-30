// src/screens/MatchesScreen.js
import React, { useState } from 'react';
import PropTypes from 'prop-types';

// Patch React.PropTypes and Swiper.propTypes to avoid missing PropTypes errors
if (!React.PropTypes) React.PropTypes = PropTypes;
const SwiperModule = require('react-native-deck-swiper');
const Swiper = SwiperModule.default;
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

import { View, Text, Image, StyleSheet, Dimensions, SafeAreaView } from 'react-native';

const { width } = Dimensions.get('window');

// Dummy profiles data
const PROFILES = [
    { id: '1', name: 'Eliza, 28', image: 'https://placekitten.com/400/500', bio: 'Love hiking and outdoor adventures.' },
    { id: '2', name: 'Mark, 30', image: 'https://placekitten.com/401/500', bio: 'Coffee fanatic and bookworm.' },
    { id: '3', name: 'Sophie, 25', image: 'https://placekitten.com/402/500', bio: 'Yoga instructor and avid traveler.' },
    { id: '4', name: 'Alex, 27', image: 'https://placekitten.com/403/500', bio: 'Musician and foodie.' },
];

export default function MatchesScreen() {
    const [cards] = useState(PROFILES);

    const onSwipedRight = (cardIndex) => {
        const matched = cards[cardIndex];
        console.log('You matched with:', matched.name);
        // TODO: persist match
    };

    const onSwipedAll = () => {
        console.log('No more profiles');
    };

    const renderCard = (card) => (
        <View style={styles.card} key={card.id}>
            <Image source={{ uri: card.image }} style={styles.cardImage} />
            <View style={styles.cardDetails}>
                <Text style={styles.cardName}>{card.name}</Text>
                <Text style={styles.cardBio}>{card.bio}</Text>
            </View>
        </View>
    );

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
                            label: { backgroundColor: 'red', color: 'white', fontSize: 24 },
                            wrapper: { flexDirection: 'column', alignItems: 'flex-end', justifyContent: 'flex-start', marginTop: 20, marginLeft: -20 },
                        },
                    },
                    right: {
                        title: 'LIKE',
                        style: {
                            label: { backgroundColor: '#4CAF50', color: 'white', fontSize: 24 },
                            wrapper: { flexDirection: 'column', alignItems: 'flex-start', justifyContent: 'flex-start', marginTop: 20, marginLeft: 20 },
                        },
                    },
                }}
                containerStyle={styles.swiper}
            />
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: '#f9f9f9', alignItems: 'center', justifyContent: 'center' },
    swiper: { flex: 1, paddingTop: 20 },
    card: { width: width * 0.9, height: width * 1.1, borderRadius: 16, backgroundColor: 'white', shadowColor: '#000', shadowOffset: { width: 0, height: 2 }, shadowOpacity: 0.1, shadowRadius: 5, elevation: 3, overflow: 'hidden' },
    cardImage: { width: '100%', height: '75%' },
    cardDetails: { padding: 16 },
    cardName: { fontSize: 22, fontWeight: '600', marginBottom: 8 },
    cardBio: { fontSize: 14, color: '#555' },
});
