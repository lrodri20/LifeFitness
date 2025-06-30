// src/screens/HomeScreen.js
import React, { useState } from 'react';
import {
    View,
    Text,
    TextInput,
    TouchableOpacity,
    StyleSheet,
    Modal,
    ScrollView,
    Switch,
    SafeAreaView,
    FlatList
} from 'react-native';
import { Picker } from '@react-native-picker/picker';
import { Ionicons } from '@expo/vector-icons';
import Slider from '@react-native-community/slider';

export default function HomeScreen() {
    const [search, setSearch] = useState('');
    const [verifyUser, setVerifyUser] = useState(null);
    const [filterVisible, setFilterVisible] = useState(false);

    // filter state
    const [maxDistanceMiles, setMaxDistanceMiles] = useState(100);
    const [minAge, setMinAge] = useState(18);
    const [maxAge, setMaxAge] = useState(30);
    const [genderPreference, setGenderPreference] = useState('Any');
    const [preferSimilarFitnessLevel, setPreferSimilarFitnessLevel] = useState(true);
    const [fitnessLevelTolerance, setFitnessLevelTolerance] = useState(3);
    const [preferHomeGym, setPreferHomeGym] = useState(true);
    const [preferPublicGym, setPreferPublicGym] = useState(true);
    const [preferOutdoor, setPreferOutdoor] = useState(true);
    const [openToGroupWorkouts, setOpenToGroupWorkouts] = useState(true);
    const [maxGroupSize, setMaxGroupSize] = useState(20);

    // dummy data
    const activities = [
        { id: 'a1', name: '5-a-side Soccer', time: 'Today, 6–7pm', spots: 8 },
        { id: 'a2', name: 'Morning Yoga', time: 'Tomorrow, 7–8am', spots: 3 }
    ];
    const people = [
        { id: 'u1', name: 'Alice', sport: 'Weightlifting' },
        { id: 'u2', name: 'Jamal', sport: 'Basketball' }
    ];

    return (
        <SafeAreaView style={styles.container}>
            {/* Search + Filter */}
            <View style={styles.searchRow}>
                <TextInput
                    style={styles.searchInput}
                    placeholder="Search activities…"
                    value={search}
                    onChangeText={setSearch}
                />
                <TouchableOpacity onPress={() => setFilterVisible(true)} style={styles.filterBtn}>
                    <Ionicons name="filter-outline" size={24} color="#555" />
                </TouchableOpacity>
            </View>

            {/* Activities List */}
            <Text style={styles.heading}>Upcoming Activities</Text>
            <FlatList
                data={activities.filter(a => a.name.toLowerCase().includes(search.toLowerCase()))}
                keyExtractor={i => i.id}
                renderItem={({ item }) => (
                    <View style={styles.card}>
                        <Text style={styles.title}>{item.name}</Text>
                        <Text>{item.time} · {item.spots} spots left</Text>
                        <TouchableOpacity style={styles.joinBtn}>
                            <Text style={styles.joinText}>Join ➤</Text>
                        </TouchableOpacity>
                    </View>
                )}
            />

            {/* People List */}
            <Text style={styles.heading}>People Looking For Partners</Text>
            <FlatList
                data={people}
                horizontal
                keyExtractor={i => i.id}
                renderItem={({ item }) => (
                    <View style={styles.personCard}>
                        <Text style={styles.personName}>{item.name}</Text>
                        <Text>{item.sport}</Text>
                        <TouchableOpacity onPress={() => setVerifyUser(item)} style={styles.connectBtn}>
                            <Text style={styles.connectText}>Connect ➤</Text>
                        </TouchableOpacity>
                    </View>
                )}
            />

            {/* Verify Modal */}
            <Modal visible={!!verifyUser} transparent animationType="slide" onRequestClose={() => setVerifyUser(null)}>
                <View style={styles.modalOverlay}>
                    <View style={styles.modalBox}>
                        <Text>Verify {verifyUser?.name}</Text>
                        <TouchableOpacity onPress={() => setVerifyUser(null)}>
                            <Text style={{ color: 'red', marginTop: 12 }}>Cancel</Text>
                        </TouchableOpacity>
                    </View>
                </View>
            </Modal>

            {/* Filter Modal */}
            <Modal visible={filterVisible} transparent
                animationType="slide"
                onRequestClose={() => setFilterVisible(false)}
                presentationStyle="overFullScreen">
                <View style={styles.modalOverlay}>
                    <View style={styles.modalBox}>
                        <View style={styles.headerRow}>
                            <Text style={styles.modalTitle}>Filters</Text>
                            <TouchableOpacity onPress={() => setFilterVisible(false)}>
                                <Ionicons name="close-outline" size={24} color="#333" />
                            </TouchableOpacity>
                        </View>
                        <ScrollView style={styles.scrollArea} contentContainerStyle={styles.scrollContent}>

                            <View style={styles.field}>
                                <Text style={styles.label}>Max Distance: {maxDistanceMiles} miles</Text>
                                <Slider
                                    style={styles.slider}
                                    minimumValue={0}
                                    maximumValue={200}
                                    step={1}
                                    value={maxDistanceMiles}
                                    onValueChange={setMaxDistanceMiles}
                                    minimumTrackTintColor="#4CAF50"
                                    maximumTrackTintColor="#d3d3d3"
                                    thumbTintColor="#4CAF50"
                                />
                            </View>
                            <View style={styles.fieldRow}>
                                <View style={styles.fieldSmall}>
                                    <Text style={styles.smallLabel}>Min Age: <Text style={styles.valueText}>{minAge}</Text></Text>
                                    <Slider
                                        style={styles.slider}
                                        minimumValue={18}
                                        maximumValue={100}
                                        step={1}
                                        value={minAge}
                                        onValueChange={setMinAge}
                                        minimumTrackTintColor="#4CAF50"
                                        maximumTrackTintColor="#d3d3d3"
                                        thumbTintColor="#4CAF50"
                                    />
                                </View>
                                <View style={styles.fieldSmall}>
                                    <Text style={styles.smallLabel}>Max Age:<Text style={styles.valueText}>{maxAge}</Text></Text>
                                    <Slider
                                        style={styles.slider}
                                        minimumValue={18}
                                        maximumValue={100}
                                        step={1}
                                        value={maxAge}
                                        onValueChange={setMaxAge}
                                        minimumTrackTintColor="#4CAF50"
                                        maximumTrackTintColor="#d3d3d3"
                                        thumbTintColor="#4CAF50"
                                    />

                                </View>
                            </View>
                            <View style={styles.field}>
                                <Text style={styles.label}>Gender Preference</Text>
                                <Picker
                                    style={styles.picker}
                                    itemStyle={styles.pickerItem}        // sets row height
                                    mode="dropdown"                      // ← key prop for iOS
                                    dropdownIconColor="#333"
                                    selectedValue={genderPreference}
                                    onValueChange={setGenderPreference}
                                >
                                    <Picker.Item label="Any" value="Any" />
                                    <Picker.Item label="Male" value="Male" />
                                    <Picker.Item label="Female" value="Female" />
                                </Picker>
                            </View>



                            <View style={styles.fieldRowCenter}>
                                <Text>Similar Fitness Level</Text>
                                <Switch
                                    value={preferSimilarFitnessLevel}
                                    onValueChange={setPreferSimilarFitnessLevel}
                                />
                            </View>
                            <View style={styles.field}>
                                <Text style={styles.label}>Fitness Tolerance: {fitnessLevelTolerance}</Text>
                                <Slider
                                    style={styles.slider}
                                    minimumValue={0}
                                    maximumValue={10}
                                    step={1}
                                    value={fitnessLevelTolerance}
                                    onValueChange={setFitnessLevelTolerance}
                                    minimumTrackTintColor="#4CAF50"
                                    maximumTrackTintColor="#d3d3d3"
                                    thumbTintColor="#4CAF50"
                                />
                            </View>
                            {
                                [
                                    { label: 'Home Gym', value: preferHomeGym, setter: setPreferHomeGym },
                                    { label: 'Public Gym', value: preferPublicGym, setter: setPreferPublicGym },
                                    { label: 'Outdoor', value: preferOutdoor, setter: setPreferOutdoor },
                                    { label: 'Group Workouts', value: openToGroupWorkouts, setter: setOpenToGroupWorkouts }
                                ].map((opt, i) => (
                                    <View key={i} style={styles.fieldRowCenter}>
                                        <Text>{opt.label}</Text>
                                        <Switch
                                            value={opt.value}
                                            onValueChange={opt.setter}
                                        />
                                    </View>
                                ))
                            }
                            <View style={styles.field}>
                                <Text style={styles.label}>Max Group Size: {maxGroupSize}</Text>
                                <Slider
                                    style={styles.slider}
                                    minimumValue={1}
                                    maximumValue={50}
                                    step={1}
                                    value={maxGroupSize}
                                    onValueChange={setMaxGroupSize}
                                    minimumTrackTintColor="#4CAF50"
                                    maximumTrackTintColor="#d3d3d3"
                                    thumbTintColor="#4CAF50"
                                />
                            </View>
                        </ScrollView>
                        <View style={styles.footerRow}>
                            <TouchableOpacity style={[styles.button, styles.cancelBtn]} onPress={() => setFilterVisible(false)}>
                                <Text style={styles.cancelText}>Cancel</Text>
                            </TouchableOpacity>
                            <TouchableOpacity style={[styles.button, styles.applyBtn]} onPress={() => setFilterVisible(false)}>
                                <Text style={styles.applyText}>Apply</Text>
                            </TouchableOpacity>
                        </View>
                    </View>
                </View>
            </Modal>
        </SafeAreaView>
    );
}

const styles = StyleSheet.create({
    container: { flex: 1, backgroundColor: '#f9f9f9', paddingTop: 32 },
    searchRow: { flexDirection: 'row', alignItems: 'center', margin: 16 },
    searchInput: { flex: 1, height: 40, borderColor: '#ccc', borderWidth: 1, borderRadius: 8, paddingHorizontal: 12 },
    filterBtn: { marginLeft: 8 },

    heading: { fontSize: 18, marginHorizontal: 16, marginVertical: 8 },
    card: { marginHorizontal: 16, marginBottom: 8, padding: 12, borderWidth: 1, borderColor: '#eee', borderRadius: 8 },
    title: { fontWeight: 'bold' },
    joinBtn: { alignSelf: 'flex-end', marginTop: 6 },
    joinText: { color: '#4CAF50' },

    personCard: { width: 140, marginLeft: 16, padding: 12, borderWidth: 1, borderColor: '#eee', borderRadius: 8 },
    personName: { fontWeight: 'bold' },
    connectBtn: { alignSelf: 'flex-end', marginTop: 6 },
    connectText: { color: '#4CAF50' },

    modalOverlay: { flex: 1, justifyContent: 'flex-end', backgroundColor: 'rgba(0,0,0,0.4)' },
    modalBox: {
        height: '80%',       // explicit percentage
        backgroundColor: '#fff',
        borderTopLeftRadius: 12,
        borderTopRightRadius: 12,
    },
    headerRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 16, borderBottomWidth: 1, borderColor: '#eee' },
    modalTitle: { fontSize: 18, fontWeight: '600', color: '#333' },

    scrollArea: { flex: 1 },
    scrollContent: { padding: 16 },

    footerRow: { flexDirection: 'row', justifyContent: 'flex-end', padding: 16, borderTopWidth: 1, borderColor: '#eee' },
    button: { paddingVertical: 12, paddingHorizontal: 24, borderRadius: 6 },
    cancelBtn: { backgroundColor: '#eee', marginRight: 12 },
    applyBtn: { backgroundColor: '#4CAF50' },
    cancelText: { color: '#333', fontSize: 14 },
    applyText: { color: '#fff', fontSize: 14 },

});
