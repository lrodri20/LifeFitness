// src/components/FilterModal.js
import React, { useState, useEffect, useContext } from 'react';
import {
    Modal,
    View,
    Text,
    TouchableOpacity,
    ScrollView,
    StyleSheet,
    Switch,
    Alert
} from 'react-native';
import Slider from '@react-native-community/slider';
import { Picker } from '@react-native-picker/picker';
import { Ionicons } from '@expo/vector-icons';
import { AuthContext } from '../context/AuthContext';
import { API_URL } from '../config';

/**
 * FilterModal
 * 
 * A self-contained modal with all filter controls built-in.
 * Props:
 *  - visible: boolean
 *  - onClose: () => void
 *  - title?: string
 */
export default function FilterModal({ visible, onClose, title = 'Filters' }) {
    const { userToken } = useContext(AuthContext);

    // Filter state
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

    // Load existing preferences when modal opens
    useEffect(() => {
        if (!visible || !userToken) return;
        (async () => {
            try {
                const resp = await fetch(`${API_URL}/api/Preferences`, {
                    headers: { Authorization: `Bearer ${userToken}` }
                });
                const prefs = await resp.json();
                if (resp.ok) {
                    setMaxDistanceMiles(prefs.maxDistanceMiles ?? 100);
                    setMinAge(prefs.minAge ?? 18);
                    setMaxAge(prefs.maxAge ?? 30);
                    setGenderPreference(prefs.genderPreference ?? 'Any');
                    setPreferSimilarFitnessLevel(prefs.preferSimilarFitnessLevel ?? true);
                    setFitnessLevelTolerance(prefs.fitnessLevelTolerance ?? 3);
                    setPreferHomeGym(prefs.preferHomeGym ?? true);
                    setPreferPublicGym(prefs.preferPublicGym ?? true);
                    setPreferOutdoor(prefs.preferOutdoor ?? true);
                    setOpenToGroupWorkouts(prefs.openToGroupWorkouts ?? true);
                    setMaxGroupSize(prefs.maxGroupSize ?? 20);
                }
            } catch (err) {
                console.warn('Failed to load preferences', err);
            }
        })();
    }, [visible, userToken]);

    // Apply and save preferences via API then close
    const applyFilters = async () => {
        const payload = {
            maxDistanceMiles,
            minAge,
            maxAge,
            genderPreference,
            preferSimilarFitnessLevel,
            fitnessLevelTolerance,
            preferHomeGym,
            preferPublicGym,
            preferOutdoor,
            openToGroupWorkouts,
            maxGroupSize,
        };
        try {
            const resp = await fetch(`${API_URL}/api/Preferences`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `Bearer ${userToken}`
                },
                body: JSON.stringify(payload)
            });
            if (!resp.ok) throw new Error('Save failed');
            Alert.alert('Success', 'Preferences saved');
        } catch (err) {
            console.error('Failed to save preferences', err);
            Alert.alert('Error', err.message);
        }
        onClose();
    };

    return (
        <Modal
            visible={visible}
            transparent
            animationType="slide"
            presentationStyle="overFullScreen"
            onRequestClose={onClose}
        >
            <View style={styles.modalOverlay}>
                <View style={styles.modalBox}>
                    <View style={styles.headerRow}>
                        <Text style={styles.modalTitle}>{title}</Text>
                        <TouchableOpacity onPress={onClose}>
                            <Ionicons name="close-outline" size={24} color="#333" />
                        </TouchableOpacity>
                    </View>
                    <ScrollView contentContainerStyle={styles.scrollContent}>
                        {/* Max Distance */}
                        <View style={styles.field}>
                            <Text style={styles.label}>Max Distance: {maxDistanceMiles} miles</Text>
                            <Slider
                                style={styles.slider}
                                minimumValue={0}
                                maximumValue={100}
                                step={1}
                                value={maxDistanceMiles}
                                onValueChange={setMaxDistanceMiles}
                                minimumTrackTintColor="#4CAF50"
                                maximumTrackTintColor="#d3d3d3"
                                thumbTintColor="#4CAF50"
                            />
                        </View>
                        {/* Age Range */}
                        <View style={styles.fieldRow}>
                            <View style={styles.halfField}>
                                <Text style={styles.label}>Min Age: {minAge}</Text>
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
                            <View style={styles.halfField}>
                                <Text style={styles.label}>Max Age: {maxAge}</Text>
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
                        {/* Gender Preference */}
                        <View style={styles.field}>
                            <Text style={styles.label}>Gender Preference</Text>
                            <View style={styles.pickerWrapper}>
                                <Picker
                                    selectedValue={genderPreference}
                                    onValueChange={setGenderPreference}
                                    mode="dropdown"
                                >
                                    <Picker.Item label="Any" value="Any" />
                                    <Picker.Item label="Male" value="Male" />
                                    <Picker.Item label="Female" value="Female" />
                                </Picker>
                            </View>
                        </View>
                        {/* Similar Fitness Level */}
                        <View style={styles.switchRow}>
                            <Text>Similar Fitness Level</Text>
                            <Switch
                                value={preferSimilarFitnessLevel}
                                onValueChange={setPreferSimilarFitnessLevel}
                            />
                        </View>
                        {/* Fitness Tolerance */}
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
                        {/* Location Preferences */}
                        {[
                            { label: 'Home Gym', value: preferHomeGym, setter: setPreferHomeGym },
                            { label: 'Public Gym', value: preferPublicGym, setter: setPreferPublicGym },
                            { label: 'Outdoor', value: preferOutdoor, setter: setPreferOutdoor },
                            { label: 'Group Workouts', value: openToGroupWorkouts, setter: setOpenToGroupWorkouts },
                        ].map((opt, i) => (
                            <View style={styles.switchRow} key={i}>
                                <Text>{opt.label}</Text>
                                <Switch value={opt.value} onValueChange={opt.setter} />
                            </View>
                        ))}
                        {/* Max Group Size */}
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
                    {/* Footer actions */}
                    <View style={styles.footerRow}>
                        <TouchableOpacity style={[styles.button, styles.cancelBtn]} onPress={onClose}>
                            <Text style={styles.cancelText}>Cancel</Text>
                        </TouchableOpacity>
                        <TouchableOpacity style={[styles.button, styles.applyBtn]} onPress={applyFilters}>
                            <Text style={styles.applyText}>Apply</Text>
                        </TouchableOpacity>
                    </View>
                </View>
            </View>
        </Modal>
    );
}

const styles = StyleSheet.create({
    modalOverlay: { flex: 1, justifyContent: 'flex-end', backgroundColor: 'rgba(0,0,0,0.4)' },
    modalBox: { backgroundColor: '#fff', borderTopLeftRadius: 12, borderTopRightRadius: 12, maxHeight: '80%' },
    headerRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', padding: 16, borderBottomWidth: 1, borderColor: '#eee' },
    modalTitle: { fontSize: 18, fontWeight: '600', color: '#333' },
    scrollContent: { padding: 16 },
    field: { marginBottom: 16 },
    label: { fontSize: 14, color: '#555', marginBottom: 8 },
    slider: { width: '100%', height: 40, marginBottom: 16 },
    fieldRow: { flexDirection: 'row', justifyContent: 'space-between' },
    halfField: { width: '48%' },
    pickerWrapper: { borderWidth: 1, borderColor: '#ccc', borderRadius: 8, overflow: 'hidden', marginBottom: 16 },
    switchRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 },
    footerRow: { flexDirection: 'row', justifyContent: 'flex-end', padding: 16, borderTopWidth: 1, borderColor: '#eee' },
    button: { paddingVertical: 12, paddingHorizontal: 24, borderRadius: 6 },
    cancelBtn: { backgroundColor: '#eee', marginRight: 12 },
    applyBtn: { backgroundColor: '#4CAF50' },
    cancelText: { color: '#333', fontSize: 14 },
    applyText: { color: '#fff', fontSize: 14 },
});
